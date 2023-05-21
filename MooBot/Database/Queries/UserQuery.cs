

using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Database;
using Moobot.Managers;

namespace MooBot.Database.Queries
{
    public static class UserQuery
    {
        public static async Task<dynamic> GetUserById(this DbSet<User> UserSet, ulong UserId, bool createIfNotExists = false)
        {
            User User = await UserSet.Where(u => u.Id == UserId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (User != default(User) || !createIfNotExists)
            {
                await dbContext.Entry(User).Collection(u => u.UserReminders).LoadAsync();
                return User;
            }

            // TODO: Look into how to do this more cleanly
            User newUser = new User { Id = UserId };
            dbContext.Add(newUser);
            await dbContext.SaveChangesAsync();

            return await UserSet.Where(g => g.Id == UserId).FirstOrDefaultAsync();
        }
    }
}
