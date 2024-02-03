using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("characters")]
    public class Character : Entity
    {
        public string Name { get; set; }

        public string Aliases { get; set; }

        public string Series { get; set; }

        public ICollection<AssignedCharacter> AssignedCharacters { get; set; }
    }
}
