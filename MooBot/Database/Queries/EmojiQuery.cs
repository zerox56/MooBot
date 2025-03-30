using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class EmojiQuery
    {
        public static async Task<dynamic> GetEmojiById(this DbSet<Emoji> emojiSet, string emojiId, bool createIfNotExists = false)
        {
            Emoji emoji = await emojiSet.Where(e => e.Id == emojiId).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            //TODO: Revision
            if (emoji != default(Emoji) || !createIfNotExists)
            {
                await dbContext.Entry(emoji).Collection(e => e.EmojiMedia).LoadAsync();
                return emoji;
            }

            Emoji newEmoji = new Emoji { Id = emojiId };
            dbContext.Add(newEmoji);
            await dbContext.SaveChangesAsync();

            return await emojiSet.Where(e => e.Id == emojiId).FirstOrDefaultAsync();
        }
    }
}
