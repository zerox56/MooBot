using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class CommandDataQuery
    {
        public static async Task<dynamic> GetCommandDataById(this DbSet<CommandData> commandDataSet, string commandId, bool createIfNotExists = false)
        {
            CommandData commandData = await commandDataSet.Where(cd => cd.Id == commandId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (commandData != default(CommandData) || !createIfNotExists)
            {
                return commandData;
            }

            // TODO: Look into how to do this more cleanly
            CommandData newCommandData = new CommandData { Id = commandId };
            dbContext.Add(newCommandData);
            await dbContext.SaveChangesAsync();

            return await commandDataSet.Where(cd => cd.Id == commandId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetMultipleCommandDataById(this DbSet<CommandData> commandDataSet, IEnumerable<string> commandIds)
        {
            return await commandDataSet.Where(cd => commandIds.Contains(cd.Id)).ToListAsync();
        }
    }
}
