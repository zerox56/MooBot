using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("user")]
    public class User : Entity
    {
        [Column("user_reminder")]
        public ICollection<UserReminder> UserReminders { get; set; }
    }
}
