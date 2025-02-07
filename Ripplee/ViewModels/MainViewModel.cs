using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Ripplee.Models;
using Ripplee.Services.Services;
using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Misc.UI;
//тут только визуалы (логика передачи данных от сюда в в userModel)
namespace Ripplee.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ChatService _chatService = new();

        [ObservableProperty]
        private UserModel user = new();

        //поменять на json
        public ObservableCollection<string> Cities { get; } = ["Москва", "Санкт-Петербург", "Новосибирск"];
        public ObservableCollection<string> Topics { get; } = ["Технологии", "Искусство", "Музыка"];

        [RelayCommand]
        private void SelectGender(string gender)
        {
            User.GenderSelection = gender;
        }

        [RelayCommand]
        private void SelectChat(string chat)
        {
            User.ChatSelection = chat;
            WeakReferenceMessenger.Default.Send(new CloseMenuMessage());
        }

        [RelayCommand]
        private async Task FindCompanion()
        {
            string result = await _chatService.FindCompanionAsync(User.GenderSelection, User.CitySelection, User.TopicSelection, User.ChatSelection);
            await Shell.Current.DisplayAlert("Результат", result, "OK");
        }
    }
}
