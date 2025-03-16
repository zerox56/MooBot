using Discord.WebSocket;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using MooBot.Configuration;
using MooBot.Modules.Handlers;

namespace MooBot.Managers
{
    public static class MessageManager
    {
        public static async Task OnMessageReceived(SocketMessage msg)
        {
            if (msg == null || msg.Author.IsBot) return;

            // Check if message comes from a guild channel with a message action enabled
            var guildId = (msg.Channel as SocketGuildChannel)?.Guild.Id ?? 0;
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            var channelSet = await dbContext.Channel.GetChannelById(msg.Channel.Id, guildId);

            if (channelSet == default(Channel)) return;

            //TODO: Dynamically check guild id + allowed channels
            if (guildId == ulong.Parse(ApplicationConfiguration.Configuration.GetSection("AssignPedia")["GuildId"]))
            {
                //TODO: Check if this even has to be awaited
                await AutoAssignHandler.AutoAssignCharacters(msg);
            }
        }
    }
}