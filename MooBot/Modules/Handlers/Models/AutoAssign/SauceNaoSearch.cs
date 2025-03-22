using System.Text.Json.Serialization;

namespace MooBot.Modules.Handlers.Models.AutoAssign
{
    public class SauceNaoSearch
    {
        [JsonPropertyName("header")]
        public SauceNaoHeader Header { get; set; }

        [JsonPropertyName("results")]
        public SauceNaoResult[] Results { get; set; }
    }

    public class SauceNaoHeader
    {
        private int shortLimit;

        [JsonPropertyName("short_limit")]
        public string ShortLimit { get { return shortLimit.ToString(); } set { shortLimit = int.Parse(value); } }

        private int longLimit;

        [JsonPropertyName("long_limit")]
        public string LongLimit { get { return longLimit.ToString(); } set { longLimit = int.Parse(value); } }

        [JsonPropertyName("long_remaining")]
        public int LongRemaining { get; set; }

        [JsonPropertyName("short_remaining")]
        public int ShortRemaining { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    public class SauceNaoResult
    {
        [JsonPropertyName("header")]
        public SauceNaoResultHeader Header { get; set; }

        [JsonPropertyName("data")]
        public SauceNaoResultData Data { get; set; }
    }

    public class SauceNaoResultHeader
    {
        private float similarity;

        [JsonPropertyName("similarity")]
        public string Similarity { get { return similarity.ToString(); } set { similarity = float.Parse(value); } }

        public float GetSimilarity()
        {
            return similarity;
        }
    }

    public class SauceNaoResultData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("material")]
        public string Material { get; set; }

        [JsonPropertyName("characters")]
        public string Characters { get; set; }
    }
}