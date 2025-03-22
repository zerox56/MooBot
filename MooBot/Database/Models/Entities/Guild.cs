using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("guild")]
    public class Guild : Entity
    {
        [Column("global_link")]
        public string GlobalLink { get; set; }

        public ICollection<Channel> Channels { get; set; }

        public ICollection<Reminder> Reminders { get; set; }

        public ICollection<Role> Roles { get; set; }
    }
}