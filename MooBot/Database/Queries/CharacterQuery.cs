using Microsoft.EntityFrameworkCore;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace MooBot.Database.Queries
{
    public static class CharacterQuery
    {
        public static async Task<dynamic> GetCharacterByIdAndName(this DbSet<Character> characterSet, ulong characterId, string name, string series = "")
        {
            Console.WriteLine(characterSet);
            var resultA = await characterSet.Where(c => c.Id == characterId).FirstOrDefaultAsync();
            var resultB = await characterSet.Where(c => c.Id == characterId &&
            c.Series.ToLower().Trim() == series.ToLower().Trim()).FirstOrDefaultAsync();
            try
            {
                var resultC = characterSet.Where(c => c.Id == characterId &&
                    c.Series.ToLower().Trim() == series.ToLower().Trim())
                    .AsEnumerable()
                    .FirstOrDefault(c => GetCharacterByName(c, name));
            }
            catch (Exception e)
            {
                Console.WriteLine("GET CHAR BY NAME ERROR");
                Console.WriteLine(e);
                return false;
            }

            return characterSet.Where(c => c.Id == characterId &&
                    c.Series.ToLower().Trim() == series.ToLower().Trim())
                    .AsEnumerable()
                    .FirstOrDefault(c => GetCharacterByName(c, name));
        }

        public static async Task<dynamic> GetOrCreateCharacter(this DbSet<Character> characterSet, string name, string aliases, string series = "")
        {
            Character character = await characterSet.Where(c => c.Series.ToLower().Trim() == series.ToLower().Trim() && c.Name.ToLower().Trim() == name.ToLower().Trim()).FirstOrDefaultAsync();

            Console.WriteLine("FOUND CHAR?");

            if (character == default(Character))
            {
                Console.WriteLine("CREATE CHAR");
                character = await CreateCharacter(characterSet, name, aliases, series);
            }

            return character;
        }

        public static async Task<dynamic> CreateCharacter(this DbSet<Character> characterSet, string name, string aliases, string series = "")
        {
            // TODO: Look into how to do this more cleanly
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            Character newCharacter = new Character { Name = name, Aliases = aliases, Series = series };
            Console.WriteLine(newCharacter);
            dbContext.Add(newCharacter);
            await dbContext.SaveChangesAsync();

            return await characterSet.Where(c => c.Name == name && c.Series == series).FirstOrDefaultAsync();
        }

        private static bool GetCharacterByName(Character character, string name)
        {
            try
            {
                var resultA = character.Name.ToLower().Trim() == name.ToLower().Trim();
                var resultB = character.Aliases.Split(',').ToList().Exists(a => a.ToLower().Trim() == name);
                return character.Name.ToLower().Trim() == name.ToLower().Trim() ||
                    character.Aliases.Split(',').ToList().Exists(a => a.ToLower().Trim() == name);
            }
            catch (Exception e)
            {
                Console.WriteLine("GET CHAR BY NAME ERROR");
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
