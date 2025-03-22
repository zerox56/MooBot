using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Configurations;
using Moobot.Database.Models.Entities;

namespace MooBot.Database.Configurations
{
    public class CharacterConfiguration : EntityConfiguration<Character>
    {
        public override void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.Property(c => c.Name).IsRequired();
            builder.Property(c => c.Aliases).HasDefaultValue("");
            builder.Property(c => c.Series).HasDefaultValue("");

            base.Configure(builder);
        }
    }
}
