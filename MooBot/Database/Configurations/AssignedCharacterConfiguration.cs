using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Configurations;
using Moobot.Database.Models.Entities;

namespace MooBot.Database.Configurations
{
    public class AssignedCharacterConfiguration : EntityConfiguration<AssignedCharacter>
    {
        public override void Configure(EntityTypeBuilder<AssignedCharacter> builder)
        {
            builder.HasKey(ac => new { ac.UserId, ac.GuildId, ac.CharacterId });
            builder.HasOne(ac => ac.User).WithMany(u => u.AssignedCharacters).HasForeignKey(ac => ac.UserId);
            builder.HasOne(ac => ac.Guild).WithMany(g => g.AssignedCharacters).HasForeignKey(ac => ac.GuildId);
            builder.HasOne(ac => ac.Character).WithMany(c => c.AssignedCharacters).HasForeignKey(ac => ac.CharacterId);

            base.Configure(builder);
        }
    }
}
