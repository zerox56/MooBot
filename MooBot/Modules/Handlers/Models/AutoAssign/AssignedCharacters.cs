using MooBot.Converts;
using System.Text.Json.Serialization;

namespace MooBot.Modules.Handlers.Models.AutoAssign
{
    public class AssignedCharacters
    {
        [JsonPropertyName("characters")]
        public Character[] Characters { get; set; }
    }

    public class Character
    {
        [JsonPropertyName("faelicanId")]
        [JsonConverter(typeof(StringToULongConverter))]
        public ulong FaelicanId { get; set; }

        [JsonPropertyName("faelicanName")]
        public string FaelicanName { get; set; }

        [JsonPropertyName("franchiseName")]
        public string FranchiseName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("booruTags")]
        public string BooruTags { get; set; }
    }
}
