using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class ReminderConfiguration : EntityConfiguration<Reminder>
    {
        public override void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.Property(r => r.Id).ValueGeneratedOnAdd();
            builder.HasOne(r => r.Channel).WithMany(c => c.Reminders).IsRequired();
            builder.Property(r => r.Title).IsRequired();
            builder.Property(r => r.Description).HasDefaultValue("");
            builder.Property(r => r.Cron).IsRequired();
            builder.Property(r => r.TimeZone).IsRequired();
            builder.Property(r => r.GifTag).HasDefaultValue("");

            base.Configure(builder);
        }
    }
}