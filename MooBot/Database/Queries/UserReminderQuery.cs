using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class UserReminderQuery
    {
        public static async Task<dynamic> CreateUserReminderByIds(this DbSet<UserReminder> userReminderSet, ulong userId, ulong reminderId)
        {
            UserReminder userReminder = await userReminderSet.Where(ur => ur.UserId == userId && ur.ReminderId == reminderId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (userReminder != default(UserReminder))
            {
                return userReminder;
            }

            // TODO: Look into how to do this more cleanly
            UserReminder newUserReminder = new UserReminder { UserId = userId, ReminderId = reminderId };
            dbContext.Add(newUserReminder);
            await dbContext.SaveChangesAsync();

            return await userReminderSet.Where(ur => ur.UserId == userId && ur.ReminderId == reminderId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetUserReminderByIds(this DbSet<UserReminder> userReminderSet, ulong userId, ulong reminderId)
        {
            return await userReminderSet.Where(ur => ur.UserId == userId && ur.ReminderId == reminderId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetUserReminderByReminder(this DbSet<UserReminder> userReminderSet, ulong reminderId)
        {
            return await userReminderSet.Where(ur => ur.ReminderId == reminderId).ToListAsync();
        }

        public static async Task<dynamic> DeleteUserReminderByIds(this DbSet<UserReminder> userReminderSet, ulong userId, ulong reminderId)
        {
            UserReminder userReminder = await userReminderSet.Where(ur => ur.UserId == userId && ur.ReminderId == reminderId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (userReminder == default(UserReminder))
            {
                return false;
            }
            dbContext.Remove(userReminder);
            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}