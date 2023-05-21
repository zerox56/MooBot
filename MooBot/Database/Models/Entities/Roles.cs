using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("role")]
    public class Role : Entity
    {
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("guild")]
        public Guild Guild { get; set; }
    }
}