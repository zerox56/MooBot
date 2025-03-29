using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class EmojiConfiguration : TimestampEntityConfiguration<Emoji>
    {
        public override void Configure(EntityTypeBuilder<Emoji> builder)
        {
            builder.HasKey(e => e.Id);

            base.Configure(builder);
        }
    }
}
