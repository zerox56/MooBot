using Moobot.Database.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace MooBot.Database.Models.Entities
{
    [Table("animal_facts")]
    public class AnimalFact : Entity
    {
        //TODO: Change to enum/entity? as long as possible to update without bot restart
        [Column("animal")]
        public string Animal { get; set; }

        [Column("fact")]
        public string Fact { get; set; }

        [Column("source")]
        public string Source { get; set; }
    }
}
