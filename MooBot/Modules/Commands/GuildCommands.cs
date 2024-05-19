using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using MooBot.Configuration;
using MooBot.Database.Queries;
using MooBot.Modules.Commands.Reminders;
using MooBot.Utils;
using System;

namespace Moobot.Modules.Commands
{
    public class GuildCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Gets the channel or global link")]
        public async Task GetLink()
        {
            if (Context.Guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            Guild guild = await dbContext.Guild.GetGuildById(Context.Guild.Id, true);

            if (guild.Channels.Count > 0 && guild.Channels.AsEnumerable().FirstOrDefault(c => c.Id == Context.Channel.Id) != null)
            {
                var channelSet = await dbContext.Channel.GetChannelById(Context.Channel.Id, Context.Guild.Id);
                await RespondAsync(channelSet.Link);
                return;
            }

            if (guild.GlobalLink == "")
            {
                await RespondAsync("No link setup on this server, make sure to run /set-link first");
                return;
            }

            await RespondAsync(guild.GlobalLink);
        }

        [SlashCommand("set-link", "Sets the channel or global link")]
        public async Task SetLink(string url)
        {
            try
            {
                if (Context.Guild == null)
                {
                    await RespondAsync("This is command is not ment for here");
                    return;
                }

                // TODO: Set default permissions to guild owner and invitee user
                if (Context.User.Id.ToString() != ApplicationConfiguration.Configuration.GetSection("Discord")["BotOwnerId"])
                {
                    await RespondAsync("You don't have permissions to use this command here", ephemeral: true);
                    return;
                }

                if (!StringUtils.IsValidUrl(url))
                {
                    await RespondAsync("The link has to be a valid url", ephemeral: true);
                    return;
                }

                var dbContext = ServiceManager.GetService<DatabaseContext>();
                Guild guild = await dbContext.Guild.GetGuildById(Context.Guild.Id, true);
                Channel channel = await dbContext.Channel.GetChannelById(Context.Channel.Id, guild.Id, true);

                channel.Link = url;
                dbContext.SaveChanges();
                await RespondAsync($"The link {url} has been set on this channel");

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await RespondAsync("Something went wrong", ephemeral: true);
            }
        }

        [SlashCommand("char-assign-settings", "Creates and assigns a user on this guild as this character")]
        public async Task SetCharacterAssignment()
        {
            if (Context.Guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            // TODO: Set default permissions to guild owner and invitee user
            if (Context.User.Id.ToString() != ApplicationConfiguration.Configuration.GetSection("Discord")["BotOwnerId"])
            {
                await RespondAsync("You don't have permissions to use this command here", ephemeral: true);
                return;
            }

            try
            {
                var component = new ComponentBuilder();
                component.WithButton(label: "Create character + user assignment", customId: "createCharAssignment", row: 0);
                await RespondAsync(text: "Manage character assignments", components: component.Build(), ephemeral: true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await RespondAsync("Something went wrong");
            }
        }

        public static async Task SetCharacterAssignment(SocketInteraction interaction)
        {
            try
            {
                var modal = await GetCharacterAssignmentModal("Set character assigment", "newCharAssign");
                await interaction.RespondWithModalAsync(modal.Build());
            }
            catch (Exception e) 
            { 
                Console.WriteLine(e); 
            }
        }

        public static async Task SetCharacterAssignmentFollowUp(SocketModal modal)
        {
            if (modal.GuildId == null)
            {
                await modal.RespondAsync("Something went wrong", ephemeral: true);
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            Guild guild = await dbContext.Guild.GetGuildById(modal.GuildId.Value, true);
            Channel channel = await dbContext.Channel.GetChannelById(modal.Channel.Id, guild.Id, true);

            var characterName = modal.Data.Components.First(d => d.CustomId == "charAssignCharName").Value.Trim();
            var characterAliases = modal.Data.Components.First(d => d.CustomId == "charAssignCharAliases").Value.Trim();
            var characterSeries = modal.Data.Components.First(d => d.CustomId == "charAssignCharSeries").Value.Trim();

            try
            {
                Console.WriteLine(characterName);
                Console.WriteLine(characterAliases);
                Console.WriteLine(characterSeries);
                Character character = await dbContext.Character.GetOrCreateCharacter(characterName, characterAliases, characterSeries);

                Console.WriteLine("CHAR CREATED");

                var userId = ulong.Parse(modal.Data.Components.First(d => d.CustomId == "charAssignUserId").Value.Trim());
                var assignedCharacter = await dbContext.AssignedCharacter.CreateAssignedCharacterByIds(userId, guild.Id, character.Id);

                if (assignedCharacter != null)
                {
                    await modal.RespondAsync($"Created the assigned character {characterName} in this guild!");
                }
                else
                {
                    await modal.RespondAsync("Something went wrong creating the assigned user", ephemeral: true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("COULD NOT CREATE CHARACTER");
                Console.WriteLine(e);
            }
        }

        private static async Task<ModalBuilder> GetCharacterAssignmentModal(string title, string customId)
        {
            return new ModalBuilder()
                .WithTitle(title)
                .WithCustomId(customId)
                .AddTextInput(
                    "Character Name",
                    "charAssignCharName",
                    required: true
                )
                .AddTextInput(
                    "Character Aliases",
                    "charAssignCharAliases"
                )
                .AddTextInput(
                    "Character Series",
                    "charAssignCharSeries"
                )
                .AddTextInput(
                    "User Id",
                    "charAssignUserId",
                    required: true
                );
        }
    }
}