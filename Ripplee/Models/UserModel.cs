using CommunityToolkit.Mvvm.ComponentModel;

namespace Ripplee.Models
{
    public partial class UserModel : ObservableObject
    {
        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string myGender = string.Empty; 

        [ObservableProperty]
        private string myCity = string.Empty; 

        // --- КРИТЕРИИ ПОИСКА СОБЕСЕДНИКА ---
        [ObservableProperty]
        private string searchGender = string.Empty; 

        [ObservableProperty]
        private string searchCity = string.Empty; 

        [ObservableProperty]
        private string searchTopic = string.Empty; 

        [ObservableProperty]
        private string chatSelection = "Голосовой чат";
    }
}