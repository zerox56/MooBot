using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class UserConfiguration : EntityConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.PingForAssignees).HasDefaultValue(false);

            base.Configure(builder);
        }
    }
}