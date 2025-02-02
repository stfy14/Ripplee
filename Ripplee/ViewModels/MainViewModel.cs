using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Ripplee.Models;
using Ripplee.Core.Services;
//тут только визуалы (логика передачи данных от сюда в в userModel)
namespace Ripplee.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ChatService _chatService = new();

        [ObservableProperty]
        public partial UserModel User { get; set; } = new();

        //поменять на json
        public ObservableCollection<string> Cities { get; } = ["Москва", "Санкт-Петербург", "Новосибирск"];
        public ObservableCollection<string> Topics { get; } = ["Технологии", "Искусство", "Музыка"];

        [RelayCommand]
        private void SelectGender(string gender)
        {
            User.GenderSelection = gender;
        }

        [RelayCommand]
        private async Task FindCompanion()
        {
            string result = await _chatService.FindCompanionAsync(User.GenderSelection, User.CitySelection, User.TopicSelection);
            await Shell.Current.DisplayAlert("Результат", result, "OK");
        }
    }
}
