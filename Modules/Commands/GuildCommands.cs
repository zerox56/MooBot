using Discord.Interactions;
using dotenv.net;
using Moobot.Database;
using Moobot.Database.Queries;
using Moobot.Managers;

namespace Moobot.Modules.Commands
{
    public class GuildCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Gets the channel or global link")]
        public async Task GetLink()
        {
            var guild = Context.Guild;
            if (guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            var guildSet = await ServiceManager.GetService<DatabaseContext>().Guild.GetGuildById(guild.Id);

            if (guildSet.GlobalLink == "")
            {
                await RespondAsync("No link setup on this server, make sure to run /setlink first");
                return;
            }

            await RespondAsync(guildSet.GlobalLink);
        }

        [SlashCommand("setlink", "Sets the channel or global link")]
        public async Task SetLink(string url, string currentChannel = "false")
        {
            try
            {
                var guild = Context.Guild;
                if (guild == null)
                {
                    await RespondAsync("This is command is not ment for here");
                    return;
                }

                // TODO: Set default permissions to guild owner and invitee user
                if (Context.User.Id.ToString() != DotEnv.Read()["BOT_OWNER"])
                {
                    await RespondAsync("You don't have permissions to use this command here", ephemeral: true);
                    return;
                }

                if (!StringUtils.IsValidUrl(url))
                {
                    await RespondAsync("The link has to be a valid url", ephemeral: true);
                    return;
                }

                var dbContext = ServiceManager.GetService<DatabaseContext>();

                var guildSet = await dbContext.Guild.GetGuildById(guild.Id, true);

                if (bool.Parse(currentChannel))
                {
                    await RespondAsync("Not supported", ephemeral: true);
                    return;
                }
                else
                {
                    guildSet.GlobalLink = url;
                    dbContext.SaveChanges();
                    await RespondAsync("The link has been set on the whole server", ephemeral: true);
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