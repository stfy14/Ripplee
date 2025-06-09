// Файл: ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Ripplee.Models;
using Ripplee.Services.Interfaces;
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService; // <-- ДОБАВЛЕНО

        // ИЗМЕНЕНО: Теперь это свойство только для чтения, оно берет данные из сервиса
        public UserModel User => _userService.CurrentUser;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MenuButtonRotation))]
        [NotifyCanExecuteChangedFor(nameof(CloseMenuCommand))]
        private bool isMenuOpen = false;

        public int MenuButtonRotation => IsMenuOpen ? 90 : 0;

        public ObservableCollection<string> Cities { get; } = new(["Москва", "Санкт-Петербург", "Новосибирск"]);
        public ObservableCollection<string> Topics { get; } = new(["Технологии", "Искусство", "Музыка"]);

        // ИЗМЕНЕНО: Конструктор теперь принимает IUserService
        public MainViewModel(IChatService chatService, IUserService userService)
        {
            Debug.WriteLine("MainViewModel constructor START"); // <-- Добавь это
            _chatService = chatService;
            _userService = userService; // <-- ДОБАВЛЕНО
            Debug.WriteLine($"MainViewModel constructor END. Username is: {User.Username}"); // <-- И это
        }

        // --- Команды ---

        [RelayCommand]
        private void ToggleMenu() // Эта команда для кнопки меню, она работает всегда
        {
            IsMenuOpen = !IsMenuOpen;
        }

        // Команда для закрытия меню по клику на фон.
        // Может выполниться, ТОЛЬКО если CanCloseMenu() вернет true.
        [RelayCommand(CanExecute = nameof(CanCloseMenu))]
        private void CloseMenu()
        {
            IsMenuOpen = false;
        }

        // Предикат, который разрешает или запрещает выполнение CloseMenuCommand
        private bool CanCloseMenu()
        {
            // Разрешаем выполнение команды только если меню открыто
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
            CloseMenu(); // Используем команду, чтобы гарантированно закрыть меню
        }

        [RelayCommand]
        private async Task FindCompanion()
        {
            var companionName = "Антон";

            // Дополняем словарь параметров для навигации
            var navigationParameters = new Dictionary<string, object>
            {
                { "name", companionName },
                { "city", User.CitySelection },   // <-- ДОБАВЛЕНО
                { "topic", User.TopicSelection }  // <-- ДОБАВЛЕНО
            };

            // Проверяем, что пользователь выбрал город и тему
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
                CloseMenu(); // Закрываем меню перед переходом
            }
            // Используем Shell-навигацию по имени маршрута
            await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
        }
    }
}