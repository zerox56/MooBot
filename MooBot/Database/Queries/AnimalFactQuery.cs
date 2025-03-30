using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;
using MooBot.Database.Models.Entities;
using System.Net.NetworkInformation;

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

        public static async Task<int> GetAnimalFactEntryNumber(this DbSet<AnimalFact> animalFactSet, AnimalFact animalFact)
        {
            return await animalFactSet
                .OrderBy(af => af.Id)
                .Where(af => af.Id < animalFact.Id && af.Animal == animalFact.Animal)
                .CountAsync() + 1; ;
        }

        public static async Task<(int, int)> GetAnimalFactCounts(this DbSet<AnimalFact> animalFactSet)
        {
            var amountOfFacts = await animalFactSet.CountAsync();
            var uniqueAnimals = await animalFactSet
                .Select(af => af.Animal)
                .Distinct()
                .CountAsync();

            return (amountOfFacts, uniqueAnimals);
        }
    }
}
