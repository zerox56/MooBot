using MooBot.Modules.Handlers.Models.AutoAssign;

namespace MooBot.Modules.Handlers.Models
{
    public class UserArtResult
    {
        public string ImageUrl { get; set; }
        public Character SelectedCharacter { get; set; }
        public string[] Characters { get; set; }
        public string[] Artists { get; set; }
    }
}
