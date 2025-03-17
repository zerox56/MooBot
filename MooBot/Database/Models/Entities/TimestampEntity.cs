using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    public abstract class TimestampEntity
    {
        [Column("date_created")]
        public DateTime DateCreated { get; set; }

        [Column("date_modified")]
        public DateTime? DateModified { get; set; }
    }
}