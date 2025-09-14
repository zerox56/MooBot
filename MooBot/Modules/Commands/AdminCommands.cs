using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using MooBot.Database.Models.Entities;
using MooBot.Modules.Commands.Reminders;
using MooBot.Preconditions;
using System;
using System.Text;
using System.Threading.Channels;

namespace MooBot.Modules.Commands
{
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("add-animal-fact", "Enables or disables being pinged for an assigned character")]
        [OwnerOnly]
        public async Task AddAnimalFact()
        {
            var modal = new ModalBuilder()
                .WithTitle("Add a new animal fact")
                .WithCustomId("addAnimalFact")
                .AddTextInput(
                    "Animal",
                    "animalFactAnimal",
                    placeholder: "Horse",
                    required: true
                )
                .AddTextInput(
                    "Fact",
                    "animalFactFact",
                    TextInputStyle.Paragraph,
                    placeholder: "Fact",
                    required: true
                )
                .AddTextInput(
                    "Source",
                    "animalFactSource",
                    placeholder: "Source",
                    required: false
            );

            await Context.Interaction.RespondWithModalAsync(modal.Build());
        }

        public static async Task AddAnimalFactFollowUp(SocketModal modal)
        {
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            var animalFact = new AnimalFact
            {
                Animal = modal.Data.Components.First(d => d.CustomId == "animalFactAnimal").Value.Trim(),
                Fact = modal.Data.Components.First(d => d.CustomId == "animalFactFact").Value.Trim(),
                Source = modal.Data.Components.First(d => d.CustomId == "animalFactSource").Value.Trim()
            };

            dbContext.AnimalFact.AddAnimalFact(animalFact);
            await modal.RespondAsync($"Animal fact has been added!");
        }

        [SlashCommand("permissions", "Checks bot permissions on the channel")]
        [OwnerOnly]
        public async Task PermissionsCheck()
        {
            var channel = Context.Channel;
            var guild = Context.Guild;
            var bot = Context.Guild.CurrentUser;

            var permissions = bot.GetPermissions(channel as IGuildChannel);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("**Channel permissions:**");
            stringBuilder.AppendLine($"- ViewChannel: {permissions.ViewChannel}");
            stringBuilder.AppendLine($"- ReadMessageHistory: {permissions.ReadMessageHistory}");
            stringBuilder.AppendLine($"- SendMessages: {permissions.SendMessages}");
            stringBuilder.AppendLine($"- ManageMessages: {permissions.ManageMessages}");
            stringBuilder.AppendLine($"- AddReactions: {permissions.AddReactions}");
            stringBuilder.AppendLine($"- AttachFiles: {permissions.AttachFiles}");

            await RespondAsync(stringBuilder.ToString());
        }
    }
}
