using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace MooBot.Modules.Handlers.Models.AutoAssign
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AssignedCharacters
    {
        public Character[] Characters { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Character
    {
        public ulong FaelicanId { get; set; }

        public string FaelicanName { get; set; }

        public string FranchiseName { get; set; }

        public string Name { get; set; }
    }
}
