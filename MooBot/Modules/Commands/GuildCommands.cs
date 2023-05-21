using Discord.Interactions;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using MooBot.Configuration;

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

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            Guild guildSet = await dbContext.Guild.GetGuildById(guild.Id);

            if (guildSet.Channels.Count > 0 && guildSet.Channels.AsEnumerable().FirstOrDefault(c => c.Id == Context.Channel.Id) != null)
            {
                var channelSet = await dbContext.Channel.GetChannelById(Context.Channel.Id);
                await RespondAsync(channelSet.Link);
                return;
            }

            if (guildSet.GlobalLink == "")
            {
                await RespondAsync("No link setup on this server, make sure to run /set-link first");
                return;
            }

            await RespondAsync(guildSet.GlobalLink);
        }

        [SlashCommand("set-link", "Sets the channel or global link")]
        public async Task SetLink(string url)
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

                var guildSet = await dbContext.Guild.GetGuildById(guild.Id, true);

                var channel = Context.Channel;
                var channelSet = await dbContext.Channel.GetChannelById(channel.Id);
                if (channelSet == null)
                {
                    channelSet = await dbContext.Channel.CreateChannelById(channel.Id, guild.Id);
                }

                channelSet.Link = url;
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