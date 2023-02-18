using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("channel")]
    public class Channel : Entity
    {
        [Column("link")]
        public string Link { get; set; }
    }
}