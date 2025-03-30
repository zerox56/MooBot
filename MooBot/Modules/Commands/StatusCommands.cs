using Discord.Interactions;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using System.Text;

namespace Moobot.Modules.Commands
{
    public class StatusCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("status-commands", "Check the status of commands usages, etc")]
        public async Task CheckCommandsStatus()
        {
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            List<CommandData> commandDatas = await dbContext.CommandData.GetMultipleCommandDataById(["saucenao_short_remaining", "saucenao_long_remaining"]);

            var shortRemaining = commandDatas.Find(cd => cd.Id == "saucenao_short_remaining").Value;
            var longRemaining = commandDatas.Find(cd => cd.Id == "saucenao_long_remaining").Value;

            await dbContext.CommandData.GetMultipleCommandDataById(["saucenao_short_remaining", "saucenao_long_remaining"]);

            var (totalFacts, uniqueAnimals) = await dbContext.AnimalFact.GetAnimalFactCounts();
            var (amountOfMedia, uniqueEmojis) = await dbContext.EmojiMedia.GetEmojiMediaCounts();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("**SauceNao:**");
            stringBuilder.AppendLine($"Short remaining: {shortRemaining} / 4");
            stringBuilder.AppendLine($"Long remaining: {longRemaining} / 100");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("**Facts:**");
            stringBuilder.AppendLine($"{totalFacts} facts from {uniqueAnimals} animals");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("**Media:**");
            stringBuilder.AppendLine($"{amountOfMedia} images/videos from {uniqueEmojis} emoji categories");

            await RespondAsync(stringBuilder.ToString());
        }
    }
}
