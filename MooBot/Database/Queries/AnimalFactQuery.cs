using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;
using MooBot.Database.Models.Entities;

namespace Moobot.Database.Queries
{
    public static class AnimalFactQuery
    {
        public static async Task AddAnimalFact(this DbSet<AnimalFact> animalFactSet, AnimalFact animalFact)
        {
            // TODO: Look into how to do this more cleanly
            try
            {
                var dbContext = ServiceManager.GetService<DatabaseContext>();
                dbContext.Add(animalFact);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        public static async Task<dynamic> GetRandomAnimalFactByAnimal(this DbSet<AnimalFact> animalFactSet, string animal = "")
        {
            return await animalFactSet
                .Where(af => af.Animal.ToLower() == animal.ToLower())
                .OrderBy(af => EF.Functions.Random())
                .FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetRandomAnimalFact(this DbSet<AnimalFact> animalFactSet)
        {
            return await animalFactSet
                .OrderBy(af => EF.Functions.Random())
                .FirstOrDefaultAsync();
        }
    }
}
