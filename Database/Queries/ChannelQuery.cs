using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Queries
{
    public static class ChannelQuery
    {
        public static async Task<dynamic> GetChannelById(this DbSet<Channel> channelSet, ulong channelId, bool createIfNotExists = false)
        {
            // TODO: Check if needed to check if channel is actually part of the Guild
            Channel channel = await channelSet.Where(c => c.Id == channelId).FirstOrDefaultAsync();
            if (channel != default(Channel) || !createIfNotExists)
            {
                return channel;
            }

            // TODO: Look into how to do this more cleanly
            var context = new DatabaseContext();
            Channel newChannel = new Channel { Id = channelId };
            context.Add(newChannel);
            await context.SaveChangesAsync();

            return await channelSet.Where(c => c.Id == channelId).FirstOrDefaultAsync();
        }
    }
}