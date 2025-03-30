using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("emoji")]
    public class Emoji : TimestampEntity
    {
        [Column("id")]
        public string Id { get; set; }
        

        [Column("emoji_media")]
        public ICollection<EmojiMedia> EmojiMedia { get; set; }
    }
}
