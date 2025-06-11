using CommunityToolkit.Mvvm.ComponentModel;

namespace Ripplee.Models
{
    public partial class UserModel : ObservableObject
    {
        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string genderSelection = string.Empty;

        [ObservableProperty]
        private string citySelection = string.Empty;

        [ObservableProperty]
        private string topicSelection = string.Empty;

        [ObservableProperty]
        private string chatSelection = "Голосовой чат";
    }
}