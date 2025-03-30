using Discord;
using Discord.WebSocket;
using Moobot.Database;
using Moobot.Managers;
using MooBot.Configuration;
using Moobot.Database.Queries;
using Moobot.Database.Models.Entities;

namespace MooBot.Managers
{
    public static class ReactionManager
    {
        public static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> msgCache, Cacheable<IMessageChannel, ulong> channelCache, SocketReaction reaction)
        {
            ProcessMediaReaction(msgCache, channelCache, reaction);
        }

        public static async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> msgCache, Cacheable<IMessageChannel, ulong> channelCache, SocketReaction reaction)
        {
            ProcessMediaReaction(msgCache, channelCache, reaction, true);
        }

        private static async void ProcessMediaReaction(Cacheable<IUserMessage, ulong> msgCache, Cacheable<IMessageChannel, ulong> channelCache, SocketReaction reaction, bool removeEntry = false)
        {
            var msg = await msgCache.GetOrDownloadAsync();
            if (reaction.Emote is Emote) return;
            if (msg.Attachments.Count == 0) return;
            if (reaction.User.Value.IsBot) return;

            var discordConfig = ApplicationConfiguration.Configuration.GetSection("Discord");
            var mediaChannelId = ulong.Parse(discordConfig["MediaChannelId"]);
            var channel = await channelCache.GetOrDownloadAsync();

            if (channel.Id != mediaChannelId) return;

            //Add entries for all attachments in database with reaction, or append if reaction is already there
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            foreach (var attachment in msg.Attachments)
            {
                if (removeEntry)
                {
                    await dbContext.Media.DeleteMediaById(attachment.Id);
                    await dbContext.EmojiMedia.DeleteEmojiMediaByIds(reaction.Emote.Name, attachment.Id);
                }
                else
                {
                    var media = new Media {
                        Id = attachment.Id,
                        Url = attachment.Url
                    };

                    await dbContext.Emoji.GetEmojiById(reaction.Emote.Name, true);
                    media = await dbContext.Media.GetMediaByObject(media, true);

                    await dbContext.EmojiMedia.CreateEmojiMediaByIds(reaction.Emote.Name, media.Id);
                }
            }
        }
    }
}