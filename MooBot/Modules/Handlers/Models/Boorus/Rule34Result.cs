using MooBot.Managers.Enums;
using System.Text.Json.Serialization;

namespace MooBot.Modules.Handlers.Models.Boorus
{
    public class Rule34Result
    {
        [JsonPropertyName("sample_url")]
        public string SampleUrl { get; set; }

        [JsonPropertyName("file_url")]
        public string FileUrl { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; }

        [JsonPropertyName("rating")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BooruRating Rating { get; set; }
    }
}
