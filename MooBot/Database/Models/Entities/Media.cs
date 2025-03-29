using System.ComponentModel.DataAnnotations.Schema;

namespace Moobot.Database.Models.Entities
{
    [Table("media")]
    public class Media : Entity
    {
        [Column("file")]
        public string File { get; set; }

        [Column("emoji_media")]
        public ICollection<EmojiMedia> EmojiMedia { get; set; }
    }
}
