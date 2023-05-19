using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class UserReminderConfiguration : IEntityTypeConfiguration<UserReminder>
    {
        public virtual void Configure(EntityTypeBuilder<UserReminder> builder)
        {
            builder.HasKey(ur => new { ur.UserId, ur.ReminderId });
            builder.HasOne(ur => ur.User).WithMany(u => u.UserReminders).HasForeignKey(ur => ur.UserId);
            builder.HasOne(ur => ur.Reminder).WithMany(r => r.UserReminders).HasForeignKey(ur => ur.ReminderId);
        }
    }
}