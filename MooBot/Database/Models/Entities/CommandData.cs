using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("command_data")]
    public class CommandData : TimestampEntity
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("type")]
        public string Type { get; set; }
    }
}
