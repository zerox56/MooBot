using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("user_reminders")]
    public class UserReminder
    {
        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("user")]
        public User User { get; set; }

        [Column("reminder_id")]
        public ulong ReminderId { get; set; }

        [Column("reminder")]
        public Reminder Reminder { get; set; }
    }
}
