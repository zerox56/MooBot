using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class DomainTrackerConfiguration : EntityConfiguration<DomainTracker>
    {
        public override void Configure(EntityTypeBuilder<DomainTracker> builder)
        {
            builder.Property(dt => dt.Group).HasConversion<string>();
            builder.HasKey(dt => dt.TrackerParameter);

            base.Configure(builder);
        }
    }
}