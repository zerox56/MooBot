using Discord.Interactions;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TwemojiSharp;

namespace MooBot.Modules.Commands.ContentManipulation
{
    [Group("mood", "Change the 'mood' of the content of your choosing")]
    public class MoodCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("emoji", "Will change emoji to select color")]
        public async Task ChangeEmojiColor(string emoji, string color)
        {
            if (!Color.TryParse(color, out Color tintColor))
            {
                await RespondAsync("No valid color provided", ephemeral: true);
                return;
            }

            string? emojiId;
            string? emojiUrl;
            var emojiExtension = ".png";
            if (emoji.Contains(":"))
            {
                emojiId = emoji.Split(':')[2];
                emojiId = emojiId.Remove(emojiId.Length - 1, 1);

                emojiExtension = emoji.Split(':')[0].Contains('a') ? ".gif" : emojiExtension;
                emojiUrl = $"https://cdn.discordapp.com/emojis/{emojiId}{emojiExtension}";
            }
            else
            {
                var twemoji = new TwemojiLib(); ;
                twemoji.ParseToList(emoji);
                emojiUrl = twemoji.ParseToList(emoji)[0].Src;
                emojiId = Path.GetFileNameWithoutExtension(emojiUrl);
            }

            var imagesPath = ApplicationConfiguration.Configuration.GetSection("Directories")["Images"];
            var emojiFile = await WebHandler.DownloadFile(emojiUrl, imagesPath);
            if (emojiFile == string.Empty)
            {
                await RespondAsync("Something went wrong with getting the emoji", ephemeral: true);
                return;
            }

            var imagePath = $"{Path.Combine(imagesPath, emojiId)}{emojiExtension}";

            Mat? tintedEmoji;
            if (emojiExtension == ".gif")
            {
                var editedImagePath = Path.Combine(Path.GetDirectoryName(imagePath), Path.GetFileNameWithoutExtension(imagePath)) + "-edited.gif";
                await TintAnimatedImage(emojiFile, tintColor.ToPixel<Rgb24>(), editedImagePath);
                await RespondWithFileAsync(editedImagePath);
                File.Delete(editedImagePath);
            }
            else
            {
                await TintStaticImage(emojiFile, tintColor.ToPixel<Rgb24>(), imagePath);
                await RespondWithFileAsync(imagePath);
            }

            File.Delete(imagePath);
        }

        [SlashCommand("colors", "Will change emoji to select color")]
        public async Task GetPossibleColors()
        {
            await RespondAsync("There's too many colors to give here.\nPlease check the list of possible colors here: https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Color.html");
        }

        private async Task TintStaticImage(string emojiFile, Rgb24 color, string imagePath)
        {
            var emojiImage = Cv2.ImRead(emojiFile, ImreadModes.Unchanged);

            var tintedImage = await TintImage(emojiImage, color);

            Cv2.ImWrite(imagePath, tintedImage);
        }

        private async Task TintAnimatedImage(string emojiFile, Rgb24 color, string imagePath)
        {
            try
            {
                using var emojiImage = Image.Load<Rgba32>(emojiFile);

                var framePaths = new List<string>();

                for (int frameIndex = 0; frameIndex < emojiImage.Frames.Count; frameIndex++)
                {
                    using var frame = emojiImage.Frames.CloneFrame(frameIndex);
                    var framePath = Path.Join(ApplicationConfiguration.Configuration.GetSection("Directories")["Processing"], $"frame_{Path.GetFileNameWithoutExtension(emojiFile)}_{frameIndex}.png");
                    frame.Save(framePath);

                    await TintStaticImage(framePath, color, framePath);

                    framePaths.Add(framePath);
                }

                var originalMetaData = emojiImage.Frames.RootFrame.Metadata.GetGifMetadata();

                using var tintedEmoji = new Image<Rgba32>(120, 120);

                var gifMetaData = tintedEmoji.Metadata.GetGifMetadata();
                gifMetaData.RepeatCount = 0;

                var metaData = tintedEmoji.Frames.RootFrame.Metadata.GetGifMetadata();
                metaData.FrameDelay = originalMetaData.FrameDelay;
                metaData.DisposalMethod = originalMetaData.DisposalMethod;

                foreach (string framePath in framePaths)
                {
                    using var frame = Image.Load<Rgba32>(framePath);

                    metaData = frame.Frames.RootFrame.Metadata.GetGifMetadata();
                    metaData.FrameDelay = originalMetaData.FrameDelay;
                    metaData.DisposalMethod = originalMetaData.DisposalMethod;

                    tintedEmoji.Frames.AddFrame(frame.Frames.RootFrame);
                }

                tintedEmoji.Frames.RemoveFrame(0);
                tintedEmoji.SaveAsGif(imagePath);

                framePaths.ForEach(f => File.Delete(f));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<Mat> TintImage(Mat emojiImage, Rgb24 color)
        {
            var tintImage = new Mat();
            emojiImage.ConvertTo(tintImage, MatType.CV_32FC3, 1.0 / 255.0);

            var tintColor = new Vec3f(color.B / 255f, color.G / 255f, color.R / 255f);

            Console.WriteLine(tintColor);

            // Get the image dimensions
            int height = tintImage.Height;
            int width = tintImage.Width;

            // Loop through each pixel and apply the tint color
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec3f pixel = tintImage.Get<Vec3f>(y, x);
                    pixel = new Vec3f(pixel.Item0 * tintColor.Item0, pixel.Item1 * tintColor.Item1, pixel.Item2 * tintColor.Item2);
                    tintImage.Set(y, x, pixel);
                }
            }

            Mat tinted8Bit = new Mat();
            tintImage.ConvertTo(tinted8Bit, MatType.CV_8UC3, 255.0);
            return tinted8Bit;
        }
    }
}
