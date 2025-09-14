using System.Text.Json.Serialization;

namespace MooBot.Modules.Handlers.Models.AutoAssign
{
    public class Franchises
    {
        [JsonPropertyName("faelicans")]
        public Franchise[] Faelicans { get; set; }
    }

    public class Franchise
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ip_name")]
        public string IpName { get; set; }
    }
}
