using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class GuildQuery
    {
        public static async Task<dynamic> GetGuildById(this DbSet<Guild> guildSet, ulong guildId, bool createIfNotExists = false)
        {
            Guild guild = await guildSet.Where(g => g.Id == guildId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (guild != default(Guild) || !createIfNotExists)
            {
                await dbContext.Entry(guild).Collection(g => g.Channels).LoadAsync();
                await dbContext.Entry(guild).Collection(g => g.Reminders).LoadAsync();
                return guild;
            }

            // TODO: Look into how to do this more cleanly
            Guild newGuild = new Guild { Id = guildId };
            dbContext.Add(newGuild);
            await dbContext.SaveChangesAsync();

            return await guildSet.Where(g => g.Id == guildId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> AddChannelToGuildById(this DbSet<Guild> guildSet, ulong guildId, Channel channel)
        {
            Guild guild = await GetGuildById(guildSet, guildId);
            guild.Channels.Add(channel);
            await ServiceManager.GetService<DatabaseContext>().SaveChangesAsync();

            return await guildSet.Where(g => g.Id == guildId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetReminders(this DbSet<Guild> guildSet, ulong guildId)
        {
            Guild guild = await GetGuildById(guildSet, guildId);
            return guild.Reminders;
        }
    }
}