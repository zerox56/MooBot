using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class RoleConfiguration : EntityConfiguration<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasOne(r => r.Guild).WithMany(g => g.Roles).HasForeignKey(r => r.GuildId);

            base.Configure(builder);
        }
    }
}