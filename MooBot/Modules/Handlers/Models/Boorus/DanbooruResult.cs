using MooBot.Managers.Enums;
using System.Text.Json.Serialization;

namespace MooBot.Modules.Handlers.Models.Boorus
{
    public class DanbooruResult
    {
        [JsonPropertyName("tag_string")]
        public string Tags { get; set; }

        [JsonPropertyName("rating")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BooruRating Rating { get; set; }

        [JsonPropertyName("file_url")]
        public string FileUrl { get; set; }

        [JsonPropertyName("large_file_url")]
        public string SampleUrl { get; set; }

        [JsonPropertyName("tag_string_general")]
        public string TagsGeneral { get; set; }

        [JsonPropertyName("tag_string_character")]
        public string TagsCharacter { get; set; }
    }
}
