using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("emoji_media")]
    public class EmojiMedia
    {
        [Column("emoji_id")]
        public string EmojiId { get; set; }

        [Column("emoji")]
        public Emoji Emoji { get; set; }

        [Column("media_id")]
        public ulong MediaId { get; set; }

        [Column("media")]
        public Media Media { get; set; }
    }
}
