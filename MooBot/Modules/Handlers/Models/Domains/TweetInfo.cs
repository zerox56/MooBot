using System.Text.Json.Serialization;

namespace MooBot.Modules.Handlers.Models.Domains
{
    public class TweetResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("tweet")]
        public TweetInfo Tweet { get; set; }
    }

    public class TweetInfo
    {
        [JsonPropertyName("media")]
        public TweetMedia Media { get; set; }
    }

    public class TweetMedia
    {
        [JsonPropertyName("photos")]
        public List<TweetPhoto> Photos { get; set; }
    }

    public class TweetPhoto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
