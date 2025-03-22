using Discord.Interactions;
using Moobot.Database.Models.Entities;
using Moobot.Database;
using Moobot.Managers;
using MooBot.Database.Queries;

namespace MooBot.Modules.Commands
{
    public class UserCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping-for-assignees", "Enables or disables being pinged for an assigned character")]
        public async Task SetPingForAssignees()
        {
            try
            {
                var dbContext = ServiceManager.GetService<DatabaseContext>();
                User user = await dbContext.User.GetUserById(Context.User.Id, true);

                user.PingForAssignees = !user.PingForAssignees;
                dbContext.SaveChanges();

                if (user.PingForAssignees)
                {
                    await RespondAsync("Pinging for assignees has been enabled for you", ephemeral: true);
                }
                else
                {
                    await RespondAsync("Pinging for assignees has been disables for you", ephemeral: true);
                }

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await RespondAsync("Something went wrong", ephemeral: true);
            }
        }
    }
}
