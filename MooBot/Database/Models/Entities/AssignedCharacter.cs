using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("assigned_characters")]
    public class AssignedCharacter
    {
        [Column("user_id")]
        public ulong UserId { get; set; }

        public User User { get; set; }

        [Column("guild_id")]
        public ulong GuildId { get; set; }

        public Guild Guild { get; set; }

        [Column("character_id")]
        public ulong CharacterId { get; set; }

        public Character Character { get; set; }
    }
}
