using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class GuildConfiguration : EntityConfiguration<Guild>
    {
        public override void Configure(EntityTypeBuilder<Guild> builder)
        {
            builder.Property(p => p.GlobalLink).HasDefaultValue("");

            base.Configure(builder);
        }
    }
}