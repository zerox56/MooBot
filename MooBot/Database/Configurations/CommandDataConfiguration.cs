using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class CommandDataConfiguration : TimestampEntityConfiguration<CommandData>
    {
        public override void Configure(EntityTypeBuilder<CommandData> builder)
        {
            builder.HasKey(cd => cd.Id);
            builder.Property(cd => cd.Value).HasDefaultValue("");
            builder.Property(cd => cd.Type).HasDefaultValue("");

            base.Configure(builder);
        }
    }
}