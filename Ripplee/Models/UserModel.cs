using CommunityToolkit.Mvvm.ComponentModel;

namespace Ripplee.Models
{
    public partial class UserModel: ObservableObject
    {
        public string Username { get; set; } = "Алексей";

        [ObservableProperty]
        public partial string? GenderSelection { get; set; }

        [ObservableProperty]
        public partial string? CitySelection { get; set; }

        [ObservableProperty]
        public partial string? TopicSelection { get; set; }
    }
}
