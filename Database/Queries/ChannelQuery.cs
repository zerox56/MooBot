using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class ChannelQuery
    {
        public static async Task<dynamic> GetChannelById(this DbSet<Channel> channelSet, ulong channelId)
        {
            // TODO: Check if needed to check if channel is actually part of the Guild
            Channel channel = await channelSet.Where(c => c.Id == channelId).FirstOrDefaultAsync();
            if (channel != default(Channel))
            {
                return channel;
            }
            else
            {
                return null;
            }
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
    }
}