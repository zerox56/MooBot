using Discord;
using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using Moobot.Managers;

namespace Moobot.Database.Queries
{
    public static class MediaQuery
    {
        public static async Task<dynamic> GetMediaByObject(this DbSet<Media> mediaSet, Media mediaObj, bool createIfNotExists = false)
        {
            Media media = await mediaSet.Where(m => m.Id == mediaObj.Id).FirstOrDefaultAsync();
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            //TODO: Revision
            if (media != default(Media) || !createIfNotExists)
            {
                await dbContext.Entry(media).Collection(m => m.EmojiMedia).LoadAsync();
                return media;
            }

            dbContext.Add(mediaObj);
            await dbContext.SaveChangesAsync();

            return await mediaSet.Where(m => m.Id == mediaObj.Id).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> GetMediaById(this DbSet<Media> mediaSet, ulong mediaId)
        {
            return await mediaSet.Where(m => m.Id == mediaId).FirstOrDefaultAsync();
        }

        public static async Task<dynamic> DeleteMediaById(this DbSet<Media> mediaSet, ulong mediaId)
        {
            Media media = await mediaSet.Where(m => m.Id == mediaId).FirstOrDefaultAsync();

            if (media == default(Media))
            {
                return false;
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            dbContext.Remove(media);
            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}
