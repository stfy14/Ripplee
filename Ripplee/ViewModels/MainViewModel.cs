using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Models;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services; // Для UserChangedMessage
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic; // Для Dictionary

namespace Ripplee.ViewModels
{
    public partial class MainViewModel : ObservableObject, IRecipient<UserChangedMessage>
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private UserModel user;

        [ObservableProperty]
        private UserModel? userCriteriaEditModel;

        public ObservableCollection<string> Cities { get; }
        public ObservableCollection<string> Topics { get; }

        [ObservableProperty]
        private bool isShowingFilterControls = true;

        // Конструктор
        public MainViewModel(IUserService userService)
        {
            _userService = userService;
            // _chatService = chatService;
            User = _userService.CurrentUser;
            User.ChatSelection = "Текстовый чат"; 

            Cities = new ObservableCollection<string>(["Москва", "Санкт-Петербург", "Новосибирск", "Екатеринбург", "Казань"]); 
            Topics = new ObservableCollection<string>(["Технологии", "Искусство", "Музыка", "Спорт", "Путешествия"]); 

            WeakReferenceMessenger.Default.Register<UserChangedMessage>(this);
        }

        public void Receive(UserChangedMessage message)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                if (message.NewUser != null)
                {
                    User = message.NewUser;

                    if (string.IsNullOrEmpty(User.ChatSelection))
                    {
                        User.ChatSelection = "Текстовый чат";
                    }
                }
                else 
                {
                    User = new UserModel { ChatSelection = "Текстовый чат" }; 
                }
            });
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

            if (string.IsNullOrEmpty(UserCriteriaEditModel.MyCity) || 
                UserCriteriaEditModel.MyCity == "Не указан" || 
                UserCriteriaEditModel.MyCity == "Выберите ваш город" || 
                string.IsNullOrEmpty(UserCriteriaEditModel.MyGender) || 
                UserCriteriaEditModel.MyGender == "Не указан") 
            {
                await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, выберите ваш город и пол.", "OK");
                return;
            }

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

        // Команды выбора (остаются как были)
        [RelayCommand]
        private void SelectMyGenderInEdit(string gender)
        {
            if (UserCriteriaEditModel != null) UserCriteriaEditModel.MyGender = gender;
        }

        [RelayCommand]
        private void SelectSearchGender(string gender)
        {
            User.SearchGender = gender;
        }

        // Команда перехода в настройки (остается)
        [RelayCommand]
        private async Task GoToSettings()
        {
            await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
        }

        // Команда поиска собеседника (ОБНОВЛЕНА для передачи типа чата)
        [RelayCommand]
        private async Task FindCompanion()
        {
            // Проверка обязательных полей профиля пользователя
            if (string.IsNullOrEmpty(User.MyCity) || User.MyCity == "Не указан" ||
                string.IsNullOrEmpty(User.MyGender) || User.MyGender == "Не указан")
            {
                await Shell.Current.DisplayAlert("Профиль не заполнен", "Укажите ваш город и пол в вашем профиле (кнопка 'Человечка' справа).", "OK");
                return;
            }

            // Проверка обязательных полей для поиска
            if (string.IsNullOrEmpty(User.SearchCity) || User.SearchCity == "Выберать город" || // Учитываем текст плейсхолдера
                string.IsNullOrEmpty(User.SearchTopic) || User.SearchTopic == "Выберать тему")  // Учитываем текст плейсхолдера
            {
                await Shell.Current.DisplayAlert("Критерии не выбраны", "Пожалуйста, выберите город и тему для поиска собеседника.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(User.SearchGender)) // Если пол для поиска не выбран, считаем "Любой"
            {
                User.SearchGender = "Любой"; // Или как у тебя называется значение по умолчанию для поиска любого пола
            }


            var navigationParameters = new Dictionary<string, object>
            {
                { "gender", User.SearchGender },
                { "city", User.SearchCity },
                { "topic", User.SearchTopic },
                { "userCity", User.MyCity }, // Город текущего пользователя
                { "userGender", User.MyGender }, // Пол текущего пользователя
                { "chatType", User.ChatSelection } // <--- ТИП ЧАТА (например, "Текстовый чат" или "Голосовой чат")
            };

            // Определяем, на какую страницу чата переходить в зависимости от User.ChatSelection
            // Пока у нас только TextChatPage готова, VoiceChatPage - заглушка.
            // В будущем, когда будет VoiceChatPage, здесь будет if/else или switch.
            string targetChatPage = nameof(Views.TextChatPage); // По умолчанию текстовый
            // if (User.ChatSelection == "Голосовой чат") {
            // targetChatPage = nameof(Views.VoiceChatPage); // Когда будет готов
            // }


            // Переходим на страницу поиска, которая потом перенаправит на нужный чат
            await Shell.Current.GoToAsync(nameof(Views.SearchingPage), true, navigationParameters);
        }
    }
}