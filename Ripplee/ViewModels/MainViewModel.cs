using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ripplee.Services;
using Ripplee.Models; 

namespace Ripplee.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ChatService _chatService = new();

        [ObservableProperty]
        private UserModel user = new(); 

        public ObservableCollection<string> Cities { get; } = new() { "Москва", "Санкт-Петербург", "Новосибирск" };
        public ObservableCollection<string> Topics { get; } = new() { "Технологии", "Искусство", "Музыка" };


        //...ЗАГЛУШКА Я НЕ ЕБУ ЧЕ ДЕЛАТЬ
        [RelayCommand]
        private void SelectGender(string gender)
        {
            User = new UserModel
            {
                GenderSelection = gender,
                CitySelection = User.CitySelection,
                TopicSelection = User.TopicSelection
            };
        }
        //...

        [RelayCommand]
        private async Task FindCompanion()
        {
            string result = await _chatService.FindCompanionAsync(User.GenderSelection, User.CitySelection, User.TopicSelection);
            await Shell.Current.DisplayAlert("Результат", result, "OK");
        }
    }
}
