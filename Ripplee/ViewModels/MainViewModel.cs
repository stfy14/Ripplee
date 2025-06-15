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
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        #region Public Properties

        [ObservableProperty]
        private UserModel user;

        // --- НОВАЯ МОДЕЛЬ ДЛЯ РЕДАКТИРОВАНИЯ ---
        // UI в панели критериев будет привязан к этой временной модели.
        [ObservableProperty]
        private UserModel? userCriteriaEditModel;

        public ObservableCollection<string> Cities { get; }
        public ObservableCollection<string> Topics { get; }

        #endregion

        #region UI State Properties

        [ObservableProperty]
        private bool isMenuOpen = false;

        [ObservableProperty]
        private bool isShowingFilterControls = true;

        #endregion

        #region Menu and Panels Logic

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        // Команда для открытия/закрытия панели критериев
        [RelayCommand]
        private void ToggleCriteriaView()
        {
            // Если мы собираемся ОТКРЫТЬ панель...
            if (IsShowingFilterControls)
            {
                // ...создаем временную копию актуальных данных пользователя.
                UserCriteriaEditModel = new UserModel
                {
                    MyGender = User.MyGender,
                    MyCity = User.MyCity
                };
            }
            // Переключаем флаг, который запускает анимацию
            IsShowingFilterControls = !IsShowingFilterControls;
        }

        // Команда для кнопки "Готово" - СОХРАНЯЕМ ИЗМЕНЕНИЯ
        [RelayCommand]
        private async Task SaveAndCloseCriteriaPanel()
        {
            if (UserCriteriaEditModel == null) return;

            // Шаг 1: Копируем данные из временной модели в основную
            User.MyGender = UserCriteriaEditModel.MyGender;
            User.MyCity = UserCriteriaEditModel.MyCity;

            // Шаг 2: Сохраняем основную модель на сервер
            await _userService.UpdateMyCriteriaAsync();

            // Шаг 3: Закрываем панель
            IsShowingFilterControls = true;
        }

        // Команда для кнопки "Назад" - ОТМЕНЯЕМ ИЗМЕНЕНИЯ
        [RelayCommand]
        private void CancelAndCloseCriteriaPanel()
        {
            // Просто закрываем панель. Временная модель UserCriteriaEditModel будет "забыта".
            // Основная модель User не была затронута, поэтому все вернется как было.
            IsShowingFilterControls = true;
        }

        #endregion

        #region Selections and Matchmaking Logic

        // --- НОВАЯ КОМАНДА ---
        // Эта команда изменяет пол во ВРЕМЕННОЙ модели
        [RelayCommand]
        private void SelectMyGenderInEdit(string gender)
        {
            if (UserCriteriaEditModel != null)
            {
                UserCriteriaEditModel.MyGender = gender;
            }
        }

        // Эта команда изменяет пол для ПОИСКА, она работает с основной моделью
        [RelayCommand]
        private void SelectSearchGender(string gender)
        {
            User.SearchGender = gender;
        }

        // Остальные команды без изменений...
        [RelayCommand]
        private void SelectChat(string chat)
        {
            User.ChatSelection = chat;
            IsMenuOpen = false;
        }

        [RelayCommand]
        private async Task GoToSettings()
        {
            if (IsMenuOpen) IsMenuOpen = false;
            await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
        }

        [RelayCommand]
        private async Task FindCompanion()
        {
            if (string.IsNullOrEmpty(User.SearchCity) || string.IsNullOrEmpty(User.SearchTopic))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, выберите город и тему для поиска.", "OK");
                return;
            }

            var navigationParameters = new Dictionary<string, object>
            {
                { "gender", User.SearchGender },
                { "city", User.SearchCity },
                { "topic", User.SearchTopic }
            };

            await Shell.Current.GoToAsync(nameof(Views.SearchingPage), true, navigationParameters);
        }

        #endregion

        #region Constructor and Messaging
        public MainViewModel(IUserService userService, IChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
            User = _userService.CurrentUser;

            Cities = new ObservableCollection<string>(["Москва", "Санкт-Петербург", "Новосибирск", "Екатеринбург", "Казань"]);
            Topics = new ObservableCollection<string>(["Технологии", "Искусство", "Музыка", "Спорт", "Путешествия"]);

            WeakReferenceMessenger.Default.Register<UserChangedMessage>(this);
        }

        public void Receive(UserChangedMessage message)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                User = _userService.CurrentUser;
            });
        }
        #endregion
    }
}