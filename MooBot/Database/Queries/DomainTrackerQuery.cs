using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;
using MooBot.Managers.Enums;

namespace Moobot.Database.Queries
{
    public static class DomainTrackerQuery
    {
        public static async Task<dynamic> GetDomainTrackersByDomainGroup(this DbSet<DomainTracker> domainTrackerSet, DomainGroupEnum domainGroup)
        {
            return await domainTrackerSet.Where(dt => dt.Group == domainGroup).ToListAsync();
        }
    }
}
