using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class EmojiMediaConfiguration : IEntityTypeConfiguration<EmojiMedia>
    {
        public virtual void Configure(EntityTypeBuilder<EmojiMedia> builder)
        {
            builder.HasKey(em => new { em.EmojiId, em.MediaId });
            builder.HasOne(em => em.Emoji).WithMany(e => e.EmojiMedia).HasForeignKey(em => em.EmojiId);
            builder.HasOne(em => em.Media).WithMany(m => m.EmojiMedia).HasForeignKey(em => em.MediaId);
        }
    }
}