using MooBot.Managers.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("domain_trackers")]
    public class DomainTracker : Entity
    {
        [Column("group")]
        public DomainGroupEnum Group { get; set; }

        [Column("tracker_parameter")]
        public string TrackerParameter { get; set; }
    }
}
