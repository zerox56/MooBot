using Discord.WebSocket;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using MooBot.Modules.Handlers;

namespace MooBot.Managers
{
    public static class MessageManager
    {
        public static async Task OnMessageReceived(SocketMessage msg)
        {
            // Skip checking if the message author is a bot
            if (msg.Author.IsBot) return;

            // Check if message comes from a guild channel with a message action enabled
            var guildId = (msg.Channel as SocketGuildChannel)?.Guild.Id ?? 0;
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            var channelSet = await dbContext.Channel.GetChannelById(msg.Channel.Id, guildId);

            Console.WriteLine("GOT MESSAGE");
            Console.WriteLine(channelSet.ToString());

            if (channelSet == default(Channel)) return;

            Console.WriteLine("TAGGING");

            await AutoTagHandler.AutoTagAttachments(msg);
        }
    }
}
