using CommunityToolkit.Mvvm.ComponentModel;

namespace Ripplee.Models
{
    public partial class UserModel: ObservableObject
    {
        public string Username { get; set; } = "Алексей";

        [ObservableProperty]
        private string genderSelection = "";

        [ObservableProperty]
        private string citySelection = "";

        [ObservableProperty]
        private string topicSelection = "";
    }
}
