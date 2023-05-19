using Discord.Interactions;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using Moobot.Modules.Commands.Reminders;
using Quartz;
using Quartz.Impl;

namespace Moobot.Modules.Commands
{
    public class ReminderCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public static async Task InitializeReminders()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            List<Reminder> reminders = await dbContext.Reminder.GetAllReminders();

            foreach (Reminder reminder in reminders)
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithCronSchedule($"0 {reminder.Cron}")
                    .Build();

                var reminderData = new Dictionary<string, Reminder>();
                reminderData.Add("Reminder", reminder);

                IJobDetail job = JobBuilder.Create<ReminderJob>()
                    .WithIdentity($"Reminder-{reminder.GuildId}-${reminder.Title}", "Reminders")
                    .SetJobData(new JobDataMap(reminderData))
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }

            await scheduler.Shutdown();
        }
    }
}
