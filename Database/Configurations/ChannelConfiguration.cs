using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class ChannelConfiguration : EntityConfiguration<Channel>
    {
        public override void Configure(EntityTypeBuilder<Channel> builder)
        {
            builder.Property(c => c.Link).HasDefaultValue("");
            builder.HasOne(c => c.Guild).WithMany(g => g.Channels).HasForeignKey(c => c.GuildId);

            base.Configure(builder);
        }
    }
}