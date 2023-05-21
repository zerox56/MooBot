using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using MooBot.Database.Queries;
using MooBot.Modules.Commands.Reminders;
using MooBot.Utils;

namespace Moobot.Modules.Commands
{
    public class ReminderCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("reminder-manage", "Manage guild reminders")]
        public async Task SendManageReminderOptions()
        {
            var guild = Context.Guild;
            if (guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            try
            {
                var component = new ComponentBuilder();
                component.WithButton(label: "Setup reminder", customId: "setupReminder", row: 0);
                component.WithButton(label: "Update reminder", customId: "updateReminder", row: 1);
                component.WithButton(label: "Add/Update reminder gifs", customId: "addUpdateReminderGif", row: 2);
                component.WithButton(label: "Delete reminder", customId: "deleteReminder", row: 3);
                await RespondAsync(text: "Manage all reminders", components: component.Build(), ephemeral: true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await RespondAsync("Something went wrong");
            }
        }

        [SlashCommand("reminder", "Interact with guild reminders")]
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
                var component = new ComponentBuilder();
                component.WithButton(label: "View all reminder", customId: "viewReminder", row: 0);
                component.WithButton(label: "Add yourself to a reminder", customId: "changeUserStatusReminder", row: 1);
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
            try
            {
                var modal = new ModalBuilder()
                    .WithTitle("Set reminder")
                    .WithCustomId("newReminder")
                    .AddTextInput("Title", "reminderTitle", placeholder: "Daily reset", required: true)
                    .AddTextInput("Description", "reminderDescription", TextInputStyle.Paragraph, "This is a daily reset", required: false)
                    .AddTextInput("Time (UTC)", "reminderTime", placeholder: "18:00", required: true)
                    .AddTextInput("Periodicity", "reminderPeriodicity", placeholder: "Daily or Weekly", required: true)
                    .AddTextInput("Day of the Week", "reminderDayOfWeek", placeholder: "Saturday", required: false);

                await interaction.RespondWithModalAsync(modal.Build());
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public static async Task CreateReminderFollowUp(SocketModal modal)
        {
            if (modal.GuildId == null)
            {
                await modal.RespondAsync("Something went wrong", ephemeral: true);
                return;
            }

            if (!Enum.TryParse(modal.Data.Components.First(d => d.CustomId == "reminderPeriodicity").Value.Trim(), out PeriodicityEnum periodicity))
            {
                await modal.RespondAsync("Didn't select a valid periodicity", ephemeral: true);
                return;
            }

            var rawDayOfWeek = modal.Data.Components.First(d => d.CustomId == "reminderDayOfWeek").Value.Trim();
            DayOfWeek dayOfWeek = DayOfWeek.Sunday;
            if (rawDayOfWeek != string.Empty)
            {
                if (!Enum.TryParse(rawDayOfWeek, out dayOfWeek))
                {
                    await modal.RespondAsync("Didn't select a valid day of the week", ephemeral: true);
                    return;
                }
            }

            var reminder = new Reminder
            {
                ChannelId = modal.Channel.Id,
                GuildId = modal.GuildId.Value,
                Title = modal.Data.Components.First(d => d.CustomId == "reminderTitle").Value.Trim(),
                Description = modal.Data.Components.First(d => d.CustomId == "reminderDescription").Value.Trim(),
                Time = modal.Data.Components.First(d => d.CustomId == "reminderTime").Value.Trim(),
                Periodicity = periodicity.ToString(),
                DayOfWeek = dayOfWeek.ToString(),
                GifTag = string.Empty
            };

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            var reminderSet = await dbContext.Reminder.CreateReminder(reminder);
            if (reminderSet != null)
            {
                await ReminderManager.AddReminder(reminder);
                await modal.RespondAsync($"Created the reminder {reminder.Title} in this channel!");
            }
            else
            {
                await modal.RespondAsync("Something went wrong creating the reminder", ephemeral: true);
            }
        }

        public static async Task UpdateReminder(SocketInteraction interaction, int reminderNum = -1)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong");
                return;
            }

            Reminder reminder = await SelectReminder(interaction, reminderNum);
            if (reminder == null)
                return;

            var modal = new ModalBuilder()
                .WithTitle("Update reminder")
                .WithCustomId("updateReminder" + reminder.Id)
                .AddTextInput("Title", "reminderTitle", value: reminder.Title, required: true)
                .AddTextInput("Description", "reminderDescription", TextInputStyle.Paragraph, value: reminder.Description, required: false)
                .AddTextInput("Time (UTC)", "reminderTime", placeholder: "18:00", required: false)
                .AddTextInput("Periodicity", "reminderPeriodicity", placeholder: "Daily or Weekly", required: true)
                .AddTextInput("Day of the Week", "reminderDayOfWeek", placeholder: "Saturday", required: false);

            await interaction.RespondWithModalAsync(modal.Build());
        }

        public static async Task UpdateReminderFollowUp(SocketModal modal, ulong reminderId)
        {
            if (modal.GuildId == null)
            {
                await modal.RespondAsync("Something went wrong", ephemeral: true);
                return;
            }

            if (!Enum.TryParse(modal.Data.Components.First(d => d.CustomId == "reminderPeriodicity").Value.Trim(), out PeriodicityEnum periodicity))
            {
                await modal.RespondAsync("Didn't select a valid periodicity", ephemeral: true);
                return;
            }

            var rawDayOfWeek = modal.Data.Components.First(d => d.CustomId == "reminderDayOfWeek").Value.Trim();
            DayOfWeek dayOfWeek = DayOfWeek.Sunday;
            if (rawDayOfWeek != string.Empty)
            {
                if (!Enum.TryParse(rawDayOfWeek, out dayOfWeek))
                {
                    await modal.RespondAsync("Didn't select a valid day of the week", ephemeral: true);
                    return;
                }
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            Reminder reminderSet = await dbContext.Reminder.GetReminderById(reminderId);
            var oldTitle = reminderSet.Title;

            var reminder = new Reminder
            {
                ChannelId = modal.Channel.Id,
                GuildId = modal.GuildId.Value,
                Title = modal.Data.Components.First(d => d.CustomId == "reminderTitle").Value.Trim(),
                Description = modal.Data.Components.First(d => d.CustomId == "reminderDescription").Value.Trim(),
                Time = modal.Data.Components.First(d => d.CustomId == "reminderTime").Value.Trim(),
                Periodicity = periodicity.ToString(),
                DayOfWeek = dayOfWeek.ToString()
            };

            dbContext.SaveChanges();
            await ReminderManager.UpdateReminder(reminderSet, oldTitle);
            await modal.RespondAsync($"Reminder {reminder.Title} has been updated!");
        }

        public static async Task UpdateGifReminder(SocketInteraction interaction, int reminderNum = -1)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong", ephemeral: true);
                return;
            }

            Reminder reminder = await SelectReminder(interaction, reminderNum);
            if (reminder == null)
                return;

            var modal = new ModalBuilder()
                .WithTitle("Change reminder gif")
                .WithCustomId("addUpdateReminderGif" + reminder.Id)
                .AddTextInput("gif tag", "reminderGif", placeholder: "reset", required: true);

            await interaction.RespondWithModalAsync(modal.Build());
        }

        public static async Task UpdateGifReminderFollowUp(SocketModal modal, ulong reminderId)
        {
            if (modal.GuildId == null)
            {
                await modal.RespondAsync("Something went wrong", ephemeral: true);
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            Reminder reminderSet = await dbContext.Reminder.GetReminderById(reminderId);
            reminderSet.GifTag = modal.Data.Components.First(d => d.CustomId == "reminderGif").Value;

            dbContext.SaveChanges();
            await ReminderManager.UpdateReminder(reminderSet, reminderSet.Title);
            await modal.RespondAsync($"Gif has been changed for the reminder {reminderSet.Title}!");
        }

        public static async Task DeleteReminder(SocketInteraction interaction, int reminderNum = -1)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong", ephemeral: true);
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
                    component.WithButton(label: reminder.Title, customId: "deleteReminder" + reminderId, row: reminderId);
                }
                await interaction.RespondAsync(text: "Choose a reminder to delete from this channel", components: component.Build(), ephemeral: true);
            }
            else
            {
                reminderNum = 0;
            }

            Reminder reminderToDelete = reminders[reminderNum];
            dbContext.Reminder.Remove(reminders[reminderNum]);
            dbContext.SaveChanges();

            await interaction.RespondAsync(text: $"Removed {reminderToDelete.Title} reminder from this channel");
        }

        public static async Task GetReminders(SocketInteraction interaction)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong", ephemeral: true);
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
                if ((PeriodicityEnum)Enum.Parse(typeof(PeriodicityEnum), reminder.Periodicity) == PeriodicityEnum.Daily)
                {
                    remindersMessage += $"\"{reminder.Title}\" posted every day at {reminder.Time}" + Environment.NewLine;
                }
                else
                {
                    remindersMessage += $"\"{reminder.Title}\" posted every {reminder.DayOfWeek} at {reminder.Time}" + Environment.NewLine;
                }
            }
            await interaction.RespondAsync(remindersMessage);
        }

        public static async Task ChangeUserReminderStatus(SocketInteraction interaction, int reminderNum = -1)
        {
            if (interaction.GuildId == null)
            {
                await interaction.RespondAsync("Something went wrong", ephemeral: true);
                return;
            }

            Reminder reminder = await SelectReminder(interaction, reminderNum);
            if (reminder == null)
                return;

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            UserReminder userReminder = await dbContext.UserReminder.GetUserReminderByIds(interaction.User.Id, reminder.Id);
            var action = "You are now being notified for this reminder";
            if (userReminder == default(UserReminder) || userReminder == null)
            {
                await dbContext.User.GetUserById(interaction.User.Id, true);
                await dbContext.UserReminder.CreateUserReminderByIds(interaction.User.Id, reminder.Id);
            }
            else
            {
                await dbContext.UserReminder.DeleteUserReminderByIds(interaction.User.Id, reminder.Id);
                action = "You won't be notified anymore for this reminder";
            }

            await interaction.RespondAsync(text: action, ephemeral: true);
        }

        private static async Task<Reminder> SelectReminder(SocketInteraction interaction, int reminderNum)
        {
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            ICollection<Reminder> remindersCollection = await dbContext.Channel.GetReminders(interaction.Channel.Id);
            List<Reminder> reminders = remindersCollection.ToList();
            if (reminders.Count == 0)
            {
                await interaction.RespondAsync("There are no reminders on this channel", ephemeral: true);
                return null;
            }
            if (reminderNum != -1)
            {
                return reminders[reminderNum];
            }
            if (reminders.Count > 1)
            {
                var component = new ComponentBuilder();
                for (int reminderId = 0; reminderId < reminders.Count; reminderId++)
                {
                    Reminder reminder = reminders[reminderId];
                    component.WithButton(label: reminder.Title, customId: "updateReminder" + reminderId, row: reminderId);
                }
                await interaction.RespondAsync(text: "Choose a reminder to update", components: component.Build(), ephemeral: true);
                return null;
            }

            return reminders[0];
        }
    }
}
