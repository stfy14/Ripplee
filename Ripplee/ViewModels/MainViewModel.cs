using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Models;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services; 
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    public partial class MainViewModel : ObservableObject, IRecipient<UserChangedMessage>
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService; 

        [ObservableProperty]
        private UserModel user;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MenuButtonRotation))]
        [NotifyCanExecuteChangedFor(nameof(CloseMenuCommand))]
        private bool isMenuOpen = false;

        public int MenuButtonRotation => IsMenuOpen ? 90 : 0;

        public ObservableCollection<string> Cities { get; } = new(["Москва", "Санкт-Петербург", "Новосибирск"]);
        public ObservableCollection<string> Topics { get; } = new(["Технологии", "Искусство", "Музыка"]);

        public MainViewModel(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService; 
            User = _userService.CurrentUser; 

            WeakReferenceMessenger.Default.Register<UserChangedMessage>(this);
        }

        public void Receive(UserChangedMessage message)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                User = _userService.CurrentUser;
                Debug.WriteLine($"MainViewModel updated via message. New user from service: '{User.Username}'");
            });
        }

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        [RelayCommand(CanExecute = nameof(CanCloseMenu))]
        private void CloseMenu()
        {
            IsMenuOpen = false;
        }

        private bool CanCloseMenu()
        {
            return IsMenuOpen;
        }

        [RelayCommand]
        private void SelectGender(string gender)
        {
            User.GenderSelection = gender;
        }

        [RelayCommand]
        private void SelectChat(string chat)
        {
            User.ChatSelection = chat;
            CloseMenu();
        }

        [RelayCommand]
        private async Task FindCompanion()
        {
            var companionName = "Антон";
            var navigationParameters = new Dictionary<string, object>
            {
                { "name", companionName },
                { "city", User.CitySelection },
                { "topic", User.TopicSelection }
            };

            if (string.IsNullOrEmpty(User.CitySelection) || string.IsNullOrEmpty(User.TopicSelection))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, выберите город и тему для поиска.", "OK");
                return;
            }
            await Shell.Current.GoToAsync(nameof(Views.VoiceChatPage), true, navigationParameters);
        }

        [RelayCommand]
        private async Task GoToSettings()
        {
            if (IsMenuOpen)
            {
                CloseMenu();
            }
            await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
        }
    }
}