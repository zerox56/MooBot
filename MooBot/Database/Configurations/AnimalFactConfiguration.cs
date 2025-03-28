using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Configurations;
using MooBot.Database.Models.Entities;

namespace MooBot.Database.Configurations
{
    public class AnimalFactConfiguration : EntityConfiguration<AnimalFact>
    {
        public override void Configure(EntityTypeBuilder<AnimalFact> builder)
        {
            builder.Property(af => af.Animal).IsRequired();
            builder.Property(af => af.Fact).IsRequired();
            builder.Property(af => af.Source).HasDefaultValue("");

            base.Configure(builder);
        }
    }
}
