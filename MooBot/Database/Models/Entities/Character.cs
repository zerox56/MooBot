using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("characters")]
    public class Character : Entity
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("aliases")]
        public string Aliases { get; set; }

        [Column("series")]
        public string Series { get; set; }

        public ICollection<AssignedCharacter> AssignedCharacters { get; set; }
    }
}
