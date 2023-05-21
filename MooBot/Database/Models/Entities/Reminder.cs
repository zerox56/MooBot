using MooBot.Utils;
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

        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("guild")]
        public Guild Guild { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("time")]
        public string Time { get; set; }

        [Column("periodicity")]
        public string Periodicity { get; set; }

        [Column("day_of_week")]
        public string DayOfWeek { get; set; }

        [Column("user_reminders")]
        public ICollection<UserReminder> UserReminders { get; set; }

        [Column("gif_tag")]
        public string GifTag { get; set; }
    }
}