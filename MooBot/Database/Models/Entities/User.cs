using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("users")]
    public class User : Entity
    {
        [Column("ping_for_assignees")]
        public bool PingForAssignees { get; set; }

        [Column("user_reminder")]
        public ICollection<UserReminder> UserReminders { get; set; }
    }
}
