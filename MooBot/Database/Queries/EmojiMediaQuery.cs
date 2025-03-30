using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class EmojiMediaQuery
    {
        public static async Task<dynamic> CreateEmojiMediaByIds(this DbSet<EmojiMedia> emojiMediaSet, string emojiId, ulong mediaId)
        {
            EmojiMedia emojiMedia = await emojiMediaSet.Where(em => em.EmojiId == emojiId && em.MediaId == mediaId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (emojiMedia != default(EmojiMedia))
            {
                return emojiMedia;
            }

            // TODO: Look into how to do this more cleanly
            EmojiMedia newEmojiMedia = new EmojiMedia { EmojiId = emojiId, MediaId = mediaId };
            dbContext.Add(newEmojiMedia);
            await dbContext.SaveChangesAsync();

            return await emojiMediaSet.Where(em => em.EmojiId == emojiId && em.MediaId == mediaId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetEmojiMediaByIds(this DbSet<EmojiMedia> emojiMediaSet, string emojiId, ulong mediaId)
        {
            return await emojiMediaSet.Where(em => em.EmojiId == emojiId && em.MediaId == mediaId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetEmojiMediaByReminder(this DbSet<EmojiMedia> emojiMediaSet, ulong mediaId)
        {
            return await emojiMediaSet.Where(em => em.MediaId == mediaId).ToListAsync();
        }

        public static async Task<dynamic> GetRandomEmojiMediaByEmoji(this DbSet<EmojiMedia> emojiMediaSet, string emojiId)
        {
            return await emojiMediaSet
                .Where(em => em.EmojiId == emojiId)
                .OrderBy(em => EF.Functions.Random())
                .FirstOrDefaultAsync();
        }

        public static async Task<(int, int)> GetEmojiMediaCounts(this DbSet<EmojiMedia> emojiMediaSet)
        {
            var amountOfMedia = await emojiMediaSet
                .Select(em => em.MediaId)
                .Distinct()
                .CountAsync();
            var uniqueEmojis = await emojiMediaSet
                .Select(em => em.EmojiId)
                .Distinct()
                .CountAsync();

            return (amountOfMedia, uniqueEmojis);
        }

        public static async Task<dynamic> DeleteEmojiMediaByIds(this DbSet<EmojiMedia> emojiMediaSet, string emojiId, ulong mediaId)
        {
            EmojiMedia emojiMedia = await emojiMediaSet.Where(em => em.EmojiId == emojiId && em.MediaId == mediaId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            if (emojiMedia == default(EmojiMedia))
            {
                return false;
            }
            dbContext.Remove(emojiMedia);
            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}