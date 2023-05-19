using Discord;
using Discord.WebSocket;
using Moobot.Database.Models.Entities;
using Moobot.Managers;
using Quartz;

namespace Moobot.Modules.Commands.Reminders
{
    public class ReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Reminder reminder = (Reminder)context.JobDetail.JobDataMap.Get("Reminder");

            var discordClient = ServiceManager.GetService<DiscordSocketClient>();
            var channel = await discordClient.GetChannelAsync(reminder.ChannelId) as ISocketMessageChannel;

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = reminder.Title,
                Description = reminder.Description
            };

            await channel.SendMessageAsync(embed: embed.Build());
            await Task.CompletedTask;
        }
    }
}
