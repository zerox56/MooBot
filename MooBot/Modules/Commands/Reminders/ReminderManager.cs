using Moobot.Database.Models.Entities;
using Moobot.Database;
using Moobot.Managers;
using Moobot.Database.Queries;
using MooBot.Utils;
using Discord.WebSocket;
using Discord;
using Moobot.Modules.Handlers;

namespace MooBot.Modules.Commands.Reminders
{
    public static class ReminderManager
    {
        private static Dictionary<string, Timer> reminders = new Dictionary<string, Timer>();

        public static async Task InitializeReminders()
        {
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            List<Reminder> reminders = await dbContext.Reminder.GetAllReminders();

            foreach (Reminder reminder in reminders)
            {
                await CreatScheduleJob(reminder);
            }
        }

        public static async Task AddReminder(Reminder reminder)
        {
            await CreatScheduleJob(reminder);
        }

        public static async Task UpdateReminder(Reminder reminder, string oldTitle)
        {
            await DeleteReminder(reminder.GuildId, oldTitle);
            await CreatScheduleJob(reminder);
        }

        public static async Task DeleteReminder(ulong guildId, string title)
        {
            var oldTimer = reminders.FirstOrDefault(r => r.Key == $"{guildId}-${title}").Value;
            oldTimer.Dispose();
            reminders.Remove($"{guildId}-${title}");
        }

        private static async Task CreatScheduleJob(Reminder reminder)
        {
            //TODO: Fix crash when multiple timers are being triggered at the same time
            var splitTime = reminder.Time.Split(':');
            DateTime now = DateTime.UtcNow;
            if ((PeriodicityEnum)Enum.Parse(typeof(PeriodicityEnum), reminder.Periodicity) == PeriodicityEnum.Daily)
            {
                TimeSpan executeTime = CalculateDailyTime(now, int.Parse(splitTime[0]), int.Parse(splitTime[1]));

                Timer timer = new Timer(Execute, reminder, executeTime, TimeSpan.FromDays(1));

                reminders[$"{reminder.GuildId}-${reminder.Title}"] = timer;
            }
            else
            {
                TimeSpan executeTime = CalculateWeeklyTime((DayOfWeek)Enum.Parse(typeof(DayOfWeek), reminder.DayOfWeek), now, int.Parse(splitTime[0]), int.Parse(splitTime[1]));

                Timer timer = new Timer(Execute, reminder, executeTime, TimeSpan.FromDays(7));

                reminders[$"{reminder.GuildId}-${reminder.Title}"] = timer;
            }
        }

        private static TimeSpan CalculateDailyTime(DateTime now, int hour, int minute)
        {
            // Get the target time for execution today
            DateTime targetTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            // If the target time has already passed for today, move it to the next day
            if (targetTime <= now)
            {
                targetTime = targetTime.AddDays(1);
            }

            // Calculate the time until the target time
            TimeSpan timeUntilExecution = targetTime - now;

            return timeUntilExecution;
        }

        private static TimeSpan CalculateWeeklyTime(DayOfWeek dayOfWeek, DateTime now, int hour, int minute)
        {
            int daysToAdd = ((int)dayOfWeek - (int)now.DayOfWeek + 7) % 7;

            DateTime targetTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            targetTime.AddDays(daysToAdd);

            if (targetTime <= now)
            {
                targetTime = targetTime.AddDays(7);
            }
            return targetTime - now;
        }

        private static async void Execute(object sender)
        {
            Reminder reminder = (Reminder)sender;

            var discordClient = ServiceManager.GetService<DiscordSocketClient>();
            var channel = await discordClient.GetChannelAsync(reminder.ChannelId) as ISocketMessageChannel;

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            var usersToPing = await dbContext.UserReminder.GetUserReminderByReminder(reminder.Id);

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = reminder.Title,
                Description = reminder.Description
            };

            if (reminder.GifTag != "")
            {
                //TODO: Add option to change randomness of gif
                embed.ImageUrl = await WebHandler.GetRandomGif(reminder.GifTag, 3);
            }

            if (usersToPing.Count > 0)
            {
                var mentions = "";
                foreach(UserReminder userReminder in usersToPing)
                {
                    mentions += $"<@{userReminder.UserId}> ";
                }
                await channel.SendMessageAsync(text: mentions, embed: embed.Build());
            }
            else
            {
                await channel.SendMessageAsync(embed: embed.Build());
            }
            await UpdateReminder(reminder, reminder.Title);
        }
    }
}
