using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("guild")]
    public class Guild : Entity
    {
        [Column("global_link")]
        public string GlobalLink { get; set; }

        public ICollection<Channel> Channels { get; set; }
    }
}