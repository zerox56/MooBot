using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class MediaConfiguration : EntityConfiguration<Media>
    {
        public override void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.Property(m => m.File);

            base.Configure(builder);
        }
    }
}
