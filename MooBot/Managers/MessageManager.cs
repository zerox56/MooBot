using Discord;
using Discord.WebSocket;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using Moobot.Utils;
using MooBot.Managers.Enums;
using MooBot.Modules.Handlers;
using System.Web;

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
            var channel = await dbContext.Channel.GetChannelById(msg.Channel.Id, guildId);

            if (channel == default(Channel)) return;

            await CheckForTrackers(msg);

            if (!channel.CheckAssignees) return;

            //TODO: Check if this even has to be awaited
            await AutoAssignHandler.AutoAssignCharacters(msg);
        }

        private static async Task CheckForTrackers(SocketMessage msg) {
            var contentUrls = StringUtils.GetAllUrls(msg.Content);

            if (contentUrls.Length == 0) return;

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            var hasQueryInUrl = false;

            foreach (var conentUrl in contentUrls)
            {
                if (!Uri.TryCreate(conentUrl, UriKind.Absolute, out var uri)) continue;

                var host = uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
                DomainGroup domainGroup = await dbContext.DomainGroup.GetDomainGroupById(host);

                var queryParams = HttpUtility.ParseQueryString(uri.Query);

                if (domainGroup != default(DomainGroup))
                {
                    List<DomainTracker> domainTrackers = await dbContext.DomainTracker.GetDomainTrackersByDomainGroup(domainGroup.Group);

                    if (domainTrackers.Count != 0)
                    {
                        hasQueryInUrl = HasTrackingParameter(uri, domainTrackers);
                    }
                }

                if (hasQueryInUrl) continue;

                List<DomainTracker> anyTrackers = await dbContext.DomainTracker.GetDomainTrackersByDomainGroup(DomainGroupEnum.Any);

                hasQueryInUrl = HasTrackingParameter(uri, anyTrackers);
            }

            if (!hasQueryInUrl) return;

            var channel = msg.Channel as IMessageChannel;

            await channel.SendMessageAsync(
                text: "Nice tracker",
                messageReference: new MessageReference(msg.Id)
            );
        }

        private static bool HasTrackingParameter(Uri uri, IEnumerable<DomainTracker> trackers)
        {
            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            foreach (var tracker in trackers)
            {
                if (queryParams[tracker.TrackerParameter] != null) return true;
            }

            return false;
        }
    }
}