using System.Drawing;
using Discord.Interactions;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using OpenCvSharp;
using TwemojiSharp;

namespace Moobot.Modules.Commands
{
    public class ImageCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mood", "Will change emoji to select color")]
        public async Task ChangeEmojiColor(string emoji, string color)
        {
            var emojiId = string.Empty;
            var emojiUrl = string.Empty;
            var emojiExtension = ".png";
            if (emoji.Contains(":"))
            {
                emojiId = emoji.Split(':')[2];
                emojiId = emojiId.Remove(emojiId.Length - 1, 1);

                emojiExtension = emoji.Split(':')[0].Contains('a') ? ".gif" : emojiExtension;
                if (emojiExtension == ".gif")
                {
                    await RespondAsync("Animated emojis aren't supported yet", ephemeral: true);
                    return;
                }
                emojiUrl = $"https://cdn.discordapp.com/emojis/{emojiId}{emojiExtension}";
            } 
            else
            {
                var twemoji = new TwemojiLib();
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

            var emojiImage = Cv2.ImRead(emojiFile, ImreadModes.Unchanged);
            var tintImage = new Mat();
            emojiImage.ConvertTo(tintImage, MatType.CV_32FC3, 1.0 / 255.0);

            var selectedColor = Color.FromName(color);
            var tintColor = new Vec3f(selectedColor.B / 255f, selectedColor.G / 255f, selectedColor.R / 255f);

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

            var imagePath = $"{Path.Combine(imagesPath, emojiId)}{emojiExtension}";
            Cv2.ImWrite(imagePath, tinted8Bit);
            await RespondWithFileAsync(imagePath);

            File.Delete(imagePath);
        }
    }
}