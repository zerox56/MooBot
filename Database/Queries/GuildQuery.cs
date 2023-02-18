using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class GuildQuery
    {
        public static async Task<dynamic> GetGuildById(this DbSet<Guild> guildSet, ulong guildId, bool createIfNotExists = false)
        {
            try
            {
                Guild guild = await guildSet.Where(g => g.Id == guildId).FirstOrDefaultAsync();
                if (guild != default(Guild) || !createIfNotExists)
                {
                    return guild;
                }

                // TODO: Look into how to do this more cleanly
                var dbContext = ServiceManager.GetService<DatabaseContext>();
                Guild newGuild = new Guild { Id = guildId };
                dbContext.Add(newGuild);
                await dbContext.SaveChangesAsync();

                return await guildSet.Where(g => g.Id == guildId).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return await guildSet.Where(g => g.Id == guildId).FirstOrDefaultAsync();
            }
        }
    }
}