using Discord;
using Discord.WebSocket;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using Moobot.Utils;
using MooBot.Managers.Enums;
using MooBot.Modules.Handlers;
using System.Text.RegularExpressions;

namespace MooBot.Managers
{
    public static class MessageManager
    {
        public static async Task OnMessageReceived(SocketMessage msg)
        {
            if (msg == null) return;

            // Check if message comes from a guild channel with a message action enabled
            var guildId = (msg.Channel as SocketGuildChannel)?.Guild.Id ?? 0;
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            var channel = await dbContext.Channel.GetChannelById(msg.Channel.Id, guildId);

            if (channel == default(Channel)) return;

            var cleanedLink = await CleanupEmbedLink(msg);

            if (!channel.CheckAssignees || cleanedLink) return;

            //TODO: Check if this even has to be awaited
            await AutoAssignHandler.AutoAssignCharacters(msg);
        }

        private static async Task<bool> CleanupEmbedLink(SocketMessage msg) {
            var contentUrls = StringUtils.GetAllUrls(msg.Content);

            if (contentUrls.Length == 0) return false;

            var containsSpoiler = StringUtils.CountOccurrences(msg.Content, "||") >= 2 || msg.Attachments.Any(a => a.IsSpoiler());
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            var responseMessage = "";
            var validUrls = false;
            var hasQueryInUrl = false;

            foreach (var conentUrl in contentUrls)
            {
                if (!Uri.TryCreate(conentUrl, UriKind.Absolute, out var uri)) continue;

                var url = uri.AbsoluteUri;
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    url = uri.GetLeftPart(UriPartial.Path);
                    hasQueryInUrl = true;
                }

                var host = uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
                DomainGroup domainGroup = await dbContext.DomainGroup.GetDomainGroupById(host);

                if (domainGroup == default(DomainGroup)) continue;

                if (!domainGroup.ConvertUrl && !hasQueryInUrl) continue;

                switch (domainGroup.Group)
                {
                    case DomainGroupEnum.Twitter:
                        validUrls = true;
                        var match = Regex.Match(uri.AbsolutePath, @"^/([^/]+)/status/(\d+)");
                        if (!match.Success) break;

                        var username = match.Groups[1].Value;
                        var tweetId = match.Groups[2].Value;

                        responseMessage += $"https://fixupx.com/{username}/status/{tweetId} ";
                        
                        break;
                    default:
                        break;
                }
            }

            if (!validUrls) return false;

            responseMessage.Trim();
            var channel = msg.Channel as IMessageChannel;

            if (hasQueryInUrl)
            {
                responseMessage = $"Nice tracker{Environment.NewLine}{responseMessage}";
            }

            if (msg is IUserMessage userMessage)
            {
                try
                {
                    await userMessage.ModifyAsync(m => m.Flags = MessageFlags.SuppressEmbeds);
                }
                catch (Discord.Net.HttpException ex)
                {
                    Console.WriteLine("No manage messages permissions");
                }
            }

            await channel.SendMessageAsync(
                text: responseMessage,
                messageReference: new MessageReference(msg.Id)
            );

            return true;
        }
    }
}