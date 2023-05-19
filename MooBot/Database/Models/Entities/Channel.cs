using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("channel")]
    public class Channel : Entity
    {
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("guild")]
        public Guild Guild { get; set; }

        [Column("link")]
        public string Link { get; set; }

        [Column("reminders")]
        public ICollection<Reminder> Reminders { get; set; }
    }
}