using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace MooBot.Modules.Handlers.Models.AutoAssign
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SauceNaoSearch
    {
        public SauceNaoHeader Header { get; set; }

        public SauceNaoResult[] Results { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SauceNaoHeader
    {
        private int shortLimit;

        public string ShortLimit { get { return shortLimit.ToString(); } set { shortLimit = int.Parse(value); } }

        private int longLimit;

        public string LongLimit { get { return longLimit.ToString(); } set { longLimit = int.Parse(value); } }

        public int LongRemaining { get; set; }

        public int ShortRemaining { get; set; }

        public int Status { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SauceNaoResult
    {
        public SauceNaoResultHeader Header { get; set; }

        public SauceNaoResultData Data { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SauceNaoResultHeader
    {
        private float similarity;

        public string Similarity { get { return similarity.ToString(); } set { similarity = float.Parse(value); } }

        public float GetSimilarity()
        {
            return similarity;
        }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class SauceNaoResultData
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Material { get; set; }

        public string Characters { get; set; }
    }
}