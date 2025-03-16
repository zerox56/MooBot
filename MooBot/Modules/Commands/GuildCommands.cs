using Discord.Interactions;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using Moobot.Utils;
using MooBot.Configuration;

namespace Moobot.Modules.Commands
{
    public class GuildCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("link", "Gets the channel or global link")]
        public async Task GetLink()
        {
            if (Context.Guild == null)
            {
                await RespondAsync("This is command is not ment for here");
                return;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            Guild guild = await dbContext.Guild.GetGuildById(Context.Guild.Id, true);

            if (guild.Channels.Count > 0 && guild.Channels.AsEnumerable().FirstOrDefault(c => c.Id == Context.Channel.Id) != null)
            {
                var channelSet = await dbContext.Channel.GetChannelById(Context.Channel.Id, Context.Guild.Id);
                await RespondAsync(channelSet.Link);
                return;
            }

            if (guild.GlobalLink == "")
            {
                await RespondAsync("No link setup on this server, make sure to run /set-link first");
                return;
            }

            await RespondAsync(guild.GlobalLink);
        }

        [SlashCommand("set-link", "Sets the channel or global link")]
        public async Task SetLink(string url)
        {
            try
            {
                if (Context.Guild == null)
                {
                    await RespondAsync("This is command is not ment for here");
                    return;
                }

                // TODO: Set default permissions to guild owner and invitee user
                if (Context.User.Id.ToString() != ApplicationConfiguration.Configuration.GetSection("Discord")["BotOwnerId"])
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
                Guild guild = await dbContext.Guild.GetGuildById(Context.Guild.Id, true);
                Channel channel = await dbContext.Channel.GetChannelById(Context.Channel.Id, guild.Id, true);

                channel.Link = url;
                dbContext.SaveChanges();
                await RespondAsync($"The link {url} has been set on this channel");

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