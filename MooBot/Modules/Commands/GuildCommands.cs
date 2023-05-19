using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using dotenv.net;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;

namespace Moobot.Modules.Commands
{
    public class GuildCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Gets the channel or global link")]
        public async Task GetLink()
        {
            var guild = Context.Guild;
            if (guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            Guild guildSet = await dbContext.Guild.GetGuildById(guild.Id);

            if (guildSet.Channels.Count > 0 && guildSet.Channels.AsEnumerable().FirstOrDefault(c => c.Id == Context.Channel.Id) != null)
            {
                var channelSet = await dbContext.Channel.GetChannelById(Context.Channel.Id);
                await RespondAsync(channelSet.Link);
                return;
            }

            if (guildSet.GlobalLink == "")
            {
                await RespondAsync("No link setup on this server, make sure to run /setlink first");
                return;
            }

            await RespondAsync(guildSet.GlobalLink);
        }

        [SlashCommand("setlink", "Sets the channel or global link")]
        public async Task SetLink(string url, string currentChannel = "false")
        {
            try
            {
                var guild = Context.Guild;
                if (guild == null)
                {
                    await RespondAsync("This is command is not ment for here");
                    return;
                }

                // TODO: Set default permissions to guild owner and invitee user
                if (Context.User.Id.ToString() != DotEnv.Read()["BOT_OWNER"])
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

                var guildSet = await dbContext.Guild.GetGuildById(guild.Id, true);

                if (bool.Parse(currentChannel))
                {
                    var channel = Context.Channel;
                    var channelSet = await dbContext.Channel.GetChannelById(channel.Id);
                    if (channelSet == null)
                    {
                        channelSet = await dbContext.Channel.CreateChannelById(channel.Id, guild.Id);
                    }

                    channelSet.Link = url;
                    dbContext.SaveChanges();
                    await RespondAsync("The link has been set on this channel", ephemeral: true);
                }
                else
                {
                    guildSet.Link = url;
                    dbContext.SaveChanges();
                    await RespondAsync("The link has been set on the whole server", ephemeral: true);
                }

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await RespondAsync("Something went wrong", ephemeral: true);
            }
        }

        [SlashCommand("reminder", "Manage guild reminders")]
        public async Task SendReminderOptions()
        {
            var guild = Context.Guild;
            if (guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            try
            {
                //TODO: Give less features list if not admin role or similair.
                //TODO: Add/Remove additional gifs option to existing reminders
                var component = new ComponentBuilder();
                component.WithButton(label: "Setup reminder", customId: "setupReminder", row: 0);
                component.WithButton(label: "View all reminder", customId: "viewReminder", row: 1);
                component.WithButton(label: "Add yourself to a reminder", customId: "changeUserStatusReminder", row: 2);
                component.WithButton(label: "Update reminder", customId: "updateReminder", row: 3);
                await RespondAsync(text: "Manage all reminders", components: component.Build(), ephemeral: true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await RespondAsync("Something went wrong");
            }
        }

        public static async Task CreateReminder(SocketInteraction interaction)
        {
            //TODO: Keep cron but make seperate or more easily adjustable field for users
            var modal = new ModalBuilder()
                .WithTitle("Set reminder")
                .WithCustomId("newReminder")
                .AddTextInput("Title", "reminderTitle", placeholder: "Daily reset", required: true)
                .AddTextInput("Description", "reminderDescription", TextInputStyle.Paragraph, "This is a daily reset", required: false)
                .AddTextInput("Cron", "reminderCron", placeholder: "0 18 * * *", required: true)
                .AddTextInput("Time zone", "reminderTimeZone", placeholder: "GMT", required: true)
                .AddTextInput("Optional gif tag", "reminderGifTag", placeholder: "reset", required: false);

            await interaction.RespondWithModalAsync(modal.Build());
        }

        public static async Task GetReminders(SocketInteraction interaction)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong");
                return;
            }
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            ICollection<Reminder> reminders = await dbContext.Guild.GetReminders(interaction.GuildId.Value);
            if (reminders.Count == 0)
            {
                await interaction.RespondAsync("There are no reminders on this server");
                return;
            }

            reminders = reminders.OrderBy(r => r.ChannelId).ToList();
            var remindersMessage = "**Reminders on this server are:**" + Environment.NewLine;
            ulong currentChannel = 0;

            foreach (Reminder reminder in reminders)
            {
                if (currentChannel != reminder.ChannelId)
                {
                    currentChannel = reminder.ChannelId;
                    remindersMessage += $"**{MentionUtils.MentionChannel(reminder.ChannelId)}:**" + Environment.NewLine;
                }
                //TODO: Convert cron to readable info
                remindersMessage += $"\"{reminder.Title}\" posted every {reminder.Cron}" + Environment.NewLine;
            }
            await interaction.RespondAsync(remindersMessage);
        }

        public static async Task CreateReminderFollowUp(SocketModal modal)
        {
            if (modal.GuildId == null)
            {
                await modal.RespondAsync("Something went wrong");
                return;
            }
            var reminder = new Reminder
            {
                ChannelId = modal.Channel.Id,
                GuildId = modal.GuildId.Value,
                Title = modal.Data.Components.First(d => d.CustomId == "reminderTitle").Value,
                Description = modal.Data.Components.First(d => d.CustomId == "reminderDescription").Value,
                //TODO: Check if cron is valid
                Cron = modal.Data.Components.First(d => d.CustomId == "reminderCron").Value,
                TimeZone = modal.Data.Components.First(d => d.CustomId == "reminderTimeZone").Value,
                GifTag = modal.Data.Components.First(d => d.CustomId == "reminderGifTag").Value
            };

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            var reminderSet = await dbContext.Reminder.CreateReminder(reminder);
            if (reminderSet != null)
            {
                await modal.RespondAsync("Created the reminder in this channel!");
            }
            else
            {
                await modal.RespondAsync("Something went wrong creating the reminder");
            }
        }

        public static async Task UpdateReminder(SocketInteraction interaction, int reminderNum = -1)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong");
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            ICollection<Reminder> remindersCollection = await dbContext.Channel.GetReminders(interaction.Channel.Id);
            List<Reminder> reminders = remindersCollection.ToList();
            if (reminders.Count == 0)
            {
                await interaction.RespondAsync("There are no reminders on this channel");
                return;
            }
            if (reminders.Count > 1 && reminderNum != -1)
            {
                var component = new ComponentBuilder();
                for (int reminderId = 0; reminderId < reminders.Count; reminderId++)
                {
                    Reminder reminder = reminders[reminderId];
                    component.WithButton(label: reminder.Title, customId: "updateReminder" + reminderId, row: reminderId);
                }
                await interaction.RespondAsync(text: "Choose a reminder to update", components: component.Build(), ephemeral: true);
            }
            else
            {
                reminderNum = 0;
            }

            Reminder selectedReminder = reminders[reminderNum];

            var modal = new ModalBuilder()
                .WithTitle("Update reminder")
                .WithCustomId("updateReminder" + selectedReminder.Id)
                .AddTextInput("Title", "reminderTitle", value: selectedReminder.Title, required: true)
                .AddTextInput("Description", "reminderDescription", TextInputStyle.Paragraph, value: selectedReminder.Description, required: false)
                .AddTextInput("Cron", "reminderCron", value: selectedReminder.Cron, required: true)
                .AddTextInput("Time zone", "reminderTimeZone", value: selectedReminder.TimeZone, required: true)
                .AddTextInput("Optional gif tag", "reminderGifTag", value: selectedReminder.GifTag, required: false);

            await interaction.RespondWithModalAsync(modal.Build());
        }

        public static async Task UpdateReminderFollowUp(SocketModal modal, ulong reminderId)
        {
            if (modal.GuildId == null)
            {
                await modal.RespondAsync("Something went wrong");
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            Reminder reminderSet = await dbContext.Reminder.GetReminderById(reminderId);
            reminderSet.Title = modal.Data.Components.First(d => d.CustomId == "reminderTitle").Value;
            reminderSet.Description = modal.Data.Components.First(d => d.CustomId == "reminderDescription").Value;
            //TODO: Check if cron is valid
            reminderSet.Cron = modal.Data.Components.First(d => d.CustomId == "reminderCron").Value;
            reminderSet.TimeZone = modal.Data.Components.First(d => d.CustomId == "reminderTimeZone").Value;
            reminderSet.GifTag = modal.Data.Components.First(d => d.CustomId == "reminderGifTag").Value;

            dbContext.SaveChanges();
            await modal.RespondAsync("Reminder has been updated!", ephemeral: true);
        }

        public static async Task ChangeUserReminderStatus(SocketInteraction interaction, int reminderNum = -1)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong");
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            ICollection<Reminder> remindersCollection = await dbContext.Channel.GetReminders(interaction.Channel.Id);
            List<Reminder> reminders = remindersCollection.ToList();
            if (reminders.Count == 0)
            {
                await interaction.RespondAsync("There are no reminders on this channel");
                return;
            }
            if (reminders.Count > 1 && reminderNum != -1)
            {
                var component = new ComponentBuilder();
                for (int reminderId = 0; reminderId < reminders.Count; reminderId++)
                {
                    Reminder reminder = reminders[reminderId];
                    component.WithButton(label: reminder.Title, customId: "changeUserStatusReminder" + reminderId, row: reminderId);
                }
                await interaction.RespondAsync(text: "Choose a reminder to add or remove yourself from", components: component.Build(), ephemeral: true);
            }
            else
            {
                reminderNum = 0;
            }

            Reminder selectedReminder = reminders[reminderNum];
            UserReminder userReminder = await dbContext.UserReminder.GetUserReminderByIds(interaction.User.Id, interaction.Channel.Id);
            var action = "You are now being notified for this reminder";
            if (userReminder != default(UserReminder))
            {
                await dbContext.UserReminder.CreateUserReminderByIds(interaction.User.Id, interaction.Channel.Id);
            }
            else
            {
                await dbContext.UserReminder.DeleteUserReminderByIds(interaction.User.Id, interaction.Channel.Id);
                action = "You won't be notified anymore for this reminder";
            }

            await interaction.RespondAsync(text: action, ephemeral: true);
        }
    }
}