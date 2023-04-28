using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class GuildConfiguration : EntityConfiguration<Guild>
    {
        public override void Configure(EntityTypeBuilder<Guild> builder)
        {
            builder.Property(g => g.GlobalLink).HasDefaultValue("");
            builder.HasMany(g => g.Channels).WithOne(c => c.Guild).HasForeignKey(c => c.GuildId);
            builder.HasMany(g => g.Roles).WithOne(r => r.Guild).HasForeignKey(r => r.GuildId);

            base.Configure(builder);
        }
    }
}