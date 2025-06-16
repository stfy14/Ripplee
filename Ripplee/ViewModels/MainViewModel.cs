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

        [RelayCommand]
        private void ToggleCriteriaView()
        {
            if (IsShowingFilterControls)
            {
                UserCriteriaEditModel = new UserModel
                {
                    MyGender = User.MyGender,
                    MyCity = User.MyCity
                };
            }
            IsShowingFilterControls = !IsShowingFilterControls;
        }

        [RelayCommand]
        private async Task SaveAndCloseCriteriaPanel()
        {
            if (UserCriteriaEditModel == null) return;

            User.MyGender = UserCriteriaEditModel.MyGender;
            User.MyCity = UserCriteriaEditModel.MyCity;

            await _userService.UpdateMyCriteriaAsync();

            IsShowingFilterControls = true;
        }

        [RelayCommand]
        private void CancelAndCloseCriteriaPanel()
        {
            IsShowingFilterControls = true;
        }

        #endregion

        #region Selections and Matchmaking Logic

        [RelayCommand]
        private void SelectMyGenderInEdit(string gender)
        {
            if (UserCriteriaEditModel != null)
            {
                UserCriteriaEditModel.MyGender = gender;
            }
        }

        [RelayCommand]
        private void SelectSearchGender(string gender)
        {
            User.SearchGender = gender;
        }

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
            if (string.IsNullOrEmpty(User.MyCity) || User.MyCity == string.Empty) 
            {
                await Shell.Current.DisplayAlert("Профиль не заполнен", "Пожалуйста, укажите ваш город в настройках профиля (иконка справа от 'Кого сегодня ищем?').", "OK");
                return;
            }

            var navigationParameters = new Dictionary<string, object>
            {
                { "gender", User.SearchGender },
                { "city", User.SearchCity },
                { "topic", User.SearchTopic },
                { "userCity", User.MyCity } 
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