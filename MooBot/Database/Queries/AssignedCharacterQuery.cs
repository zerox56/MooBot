using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;
using MooBot.Database.Queries;

namespace Moobot.Database.Queries
{
    public static class AssignedCharacterQuery
    {
        public static async Task<dynamic> CreateAssignedCharacterByIds(this DbSet<AssignedCharacter> assignedCharacterSet, ulong userId, ulong guildId, ulong characterId)
        {
            AssignedCharacter assignedCharacter = await assignedCharacterSet.Where(ac => ac.UserId == userId && ac.GuildId == guildId && ac.CharacterId == characterId).FirstOrDefaultAsync();

            if (assignedCharacter != default(AssignedCharacter))
            {
                return assignedCharacter;
            }

            // TODO: Look into how to do this more cleanly
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            AssignedCharacter newAssignedCharacter = new AssignedCharacter { UserId = userId, GuildId = guildId, CharacterId = characterId };
            dbContext.Add(newAssignedCharacter);
            await dbContext.SaveChangesAsync();

            return await assignedCharacterSet.Where(ac => ac.UserId == userId && ac.GuildId == guildId && ac.CharacterId == characterId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic?> GetAssignedCharacterByName(this DbSet<AssignedCharacter> assignedCharacterSet, ulong guildId, string name, string series = "")
        {
            var assignedCharacters = await assignedCharacterSet.Where(ac => ac.GuildId == guildId).ToListAsync();

            if (assignedCharacters.Count == 0) return null;

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            // TODO: See if this can be done with LINQ
            foreach (var assignedCharacter in assignedCharacters)
            {
                if (await dbContext.Character.GetCharacterByIdAndName(assignedCharacter.CharacterId, name, series) == default(Character)) continue;

                return assignedCharacter;
            }

            return null;
        }
    }
}
