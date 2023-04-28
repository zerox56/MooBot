using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("reminder")]
    public class Reminder : Entity
    {
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Column("channel")]
        public Channel Channel { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("cron")]
        public string Cron { get; set; }

        [Column("time_zone")]
        public string TimeZone { get; set; }

        [Column("user_reminders")]
        public ICollection<UserReminder> UserReminders { get; set; }

        [Column("gif_tag")]
        public string GifTag { get; set; }
    }
}