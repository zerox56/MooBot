using Moobot.Database.Models.Entities;

namespace MooBot.Managers.CharacterAssignment
{
    public class CharacterAssignment
    {
        public string Name { get; set; }

        public string Series { get; set; }

        public User User { get; set; }

        public string UserName { get; set; }
    }
}