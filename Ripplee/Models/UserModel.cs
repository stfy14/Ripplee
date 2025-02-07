using CommunityToolkit.Mvvm.ComponentModel;

namespace Ripplee.Models
{
    public partial class UserModel : ObservableObject
    {
        public string Username { get; set; } = "Алексей";

        [ObservableProperty]
        public partial string GenderSelection { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CitySelection { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string TopicSelection { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ChatSelection { get; set; } = "Голосовой чат";
    }
}