using MooBot.Managers.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("domain_group")]
    public class DomainGroup : TimestampEntity
    {
        [Column("domain")]
        public string Domain { get; set; }

        [Column("group")]
        public DomainGroupEnum Group { get; set; }
    }
}
