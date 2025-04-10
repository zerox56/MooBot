using Microsoft.EntityFrameworkCore;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Queries
{
    public static class DomainGroupQuery
    {
        public static async Task<dynamic> GetDomainGroupById(this DbSet<DomainGroup> domainGroupSet, string domain)
        {
            return await domainGroupSet.Where(dg => dg.Domain.ToLower() == domain.ToLower()).FirstOrDefaultAsync();
        }
    }
}
