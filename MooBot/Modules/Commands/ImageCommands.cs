using System.Drawing;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Moobot.Database.Models.Entities;
using Moobot.Managers;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using OpenCvSharp;

namespace Moobot.Modules.Commands
{
    public class ImageCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mood", "Will change emoji to select color")]
        public async Task ChangeEmojiColor(string emoji, string color)
        {
            var emojiId = emoji.Split(':')[2];
            emojiId = emojiId.Remove(emojiId.Length - 1, 1);
            var emojiUrl = $"https://cdn.discordapp.com/emojis/{emojiId}.png";
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

            Cv2.ImWrite($"{Path.Combine(imagesPath, emojiId)}.png", tinted8Bit);
            await RespondWithFileAsync($"{Path.Combine(imagesPath, emojiId)}.png");
        }
    }
}