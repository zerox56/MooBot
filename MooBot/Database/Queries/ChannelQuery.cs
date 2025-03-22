using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class ChannelQuery
    {
        public static async Task<dynamic> GetChannelById(this DbSet<Channel> channelSet, ulong channelId, ulong guildId, bool createIfNotExists = false)
        {
            Channel channel = await channelSet.Where(c => c.Id == channelId && c.GuildId == guildId).FirstOrDefaultAsync();

            if (channel == default(Channel))
            {
                if (createIfNotExists)
                {
                    return await CreateChannelById(channelSet, channelId, guildId);
                }
                return channel;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            await dbContext.Entry(channel).Collection(c => c.Reminders).LoadAsync();
            return channel;
        }

        public static async Task<dynamic> CreateChannelById(this DbSet<Channel> channelSet, ulong channelId, ulong guildId)
        {
            // TODO: Look into how to do this more cleanly
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            Channel newChannel = new Channel { Id = channelId, GuildId = guildId };
            dbContext.Add(newChannel);
            await dbContext.SaveChangesAsync();

            return await channelSet.Where(c => c.Id == channelId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetReminders(this DbSet<Channel> channelSet, ulong channelId, ulong guildId)
        {
            Channel channel = await GetChannelById(channelSet, channelId, guildId);
            return channel.Reminders;
        }
    }
}