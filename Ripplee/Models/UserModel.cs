using CommunityToolkit.Mvvm.ComponentModel;

namespace Ripplee.Models
{
    public class UserModel
    {
        public string Username { get; set; } = "Алексей";
        public string GenderSelection { get; set; } = "";
        public string CitySelection { get; set; } = "";
        public string TopicSelection { get; set; } = "";
    }
}
