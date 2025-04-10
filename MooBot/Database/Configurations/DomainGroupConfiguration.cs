using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class DomainGroupConfiguration : TimestampEntityConfiguration<DomainGroup>
    {
        public override void Configure(EntityTypeBuilder<DomainGroup> builder)
        {
            builder.HasKey(dg => dg.Domain);
            builder.Property(dg => dg.Group).HasConversion<string>();

            base.Configure(builder);
        }
    }
}