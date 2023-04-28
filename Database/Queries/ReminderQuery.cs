using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class ReminderQuery
    {
        public static async Task<dynamic> CreateReminder(this DbSet<Reminder> reminderSet, Reminder reminder)
        {
            // TODO: Look into how to do this more cleanly
            try
            {
                var dbContext = ServiceManager.GetService<DatabaseContext>();
                dbContext.Add(reminder);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }

            return await reminderSet.Where(r => r.Id == reminder.Id).FirstOrDefaultAsync();
        }
    }
}