using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Services.Interfaces; 
using System.Diagnostics;       
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching; 

namespace Ripplee.ViewModels
{
    [QueryProperty(nameof(SearchGender), "gender")]
    [QueryProperty(nameof(SearchCity), "city")]
    [QueryProperty(nameof(SearchTopic), "topic")]
    [QueryProperty(nameof(UserCity), "userCity")]
    [QueryProperty(nameof(UserGender), "userGender")] 
    [QueryProperty(nameof(ChatType), "chatType")]
    public partial class SearchingViewModel : ObservableObject
    {
        private readonly ISignalRService _signalRService;
        private readonly IUserService _userService; 
        private IDispatcherTimer? _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private readonly IDispatcher _dispatcher; 

        [ObservableProperty] private string searchGender = string.Empty;
        [ObservableProperty] private string searchCity = string.Empty;
        [ObservableProperty] private string searchTopic = string.Empty;
        [ObservableProperty] private string userCity = string.Empty;
        [ObservableProperty] private string userGender = string.Empty; // <--- НОВОЕ свойство
        [ObservableProperty] private string chatType = string.Empty;   // <--- НОВОЕ свойство

        [ObservableProperty] private string timeElapsed = "0:00";
        [ObservableProperty] private string statusMessage = "Подключаемся к серверу...";
        [ObservableProperty] private bool isFindAnyoneButtonVisible = false;
        [ObservableProperty] private bool isSearching = false; 
        [ObservableProperty] private bool findAnyoneActivated = false;

        public SearchingViewModel(ISignalRService signalRService, IUserService userService, IDispatcher dispatcher)
        {
            _signalRService = signalRService;
            _userService = userService;
            _dispatcher = dispatcher; 
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsSearching = true;
            IsFindAnyoneButtonVisible = false;
            FindAnyoneActivated = false;
            _elapsedTime = TimeSpan.Zero;
            TimeElapsed = "0:00";
            StatusMessage = "Подключение к серверу...";


            if (string.IsNullOrEmpty(UserCity) || UserCity == "Не указан" ||
                        string.IsNullOrEmpty(UserGender) || UserGender == "Не указан")
            {
                await Shell.Current.DisplayAlert("Ошибка", "Ваш город и пол не указаны. Пожалуйста, укажите их в профиле.", "OK");
                await CancelSearch();
                return;
            }
            if (string.IsNullOrEmpty(ChatType)) // Если тип чата не передан, по умолчанию
            {
                ChatType = "Текстовый чат";
            }

            // Обновляем вызов ConnectAsync
            await _signalRService.ConnectAsync(
                OnCompanionFound,
                OnSearchStatusUpdate,
                OnCallEndedByPartner_NotInCall,
                OnPartnerMuteStatusChanged_NotInCall,
                OnReceiveTextMessage_NotInSearch
            );

            if (_signalRService.IsConnected)
            {
                StatusMessage = "Ищем идеального собеседника...";
                await _signalRService.FindCompanionAsync(UserCity, UserGender, SearchGender, SearchCity, SearchTopic, ChatType);
                StartTimer();
            }
            else
            {
                StatusMessage = "Не удалось подключиться к серверу поиска.";
                IsSearching = false;
            }
        }

        private Task OnReceiveTextMessage_NotInSearch(string sender, string msg, string? avatarUrl)
        {
            Debug.WriteLine("SearchingViewModel: Received OnReceiveTextMessage, but not in a chat state. Ignoring.");
            return Task.CompletedTask;
        }

        private Task OnPartnerMuteStatusChanged_NotInCall(bool isMuted)
        {
            Debug.WriteLine("SearchingViewModel: Received OnPartnerMuteStatusChanged, but not in a call state. Ignoring.");
            return Task.CompletedTask;
        }

        private async Task OnCompanionFound(string name, string city, string topic, string callGroupId, string? companionAvatarUrl)
        {
            IsSearching = false;
            StopTimer();

            string? fullAvatarUrl = null; 
            if (!string.IsNullOrEmpty(companionAvatarUrl))
            {
                string baseAddress = MauiProgram.GetApiBaseAdress();
                fullAvatarUrl = baseAddress.TrimEnd('/') + companionAvatarUrl;
            }
            Debug.WriteLine($"SearchingVM: Companion found: {name}. Avatar URL from server: {companionAvatarUrl}, Full URL for client: {fullAvatarUrl}");

            await _dispatcher.DispatchAsync(async () =>
            {
                var navigationParameters = new Dictionary<string, object>
                {
                    { "name", name },
                    { "city", city }, // Это город, по которому сошлись (может быть город собеседника или общий)
                    { "topic", topic },// Это тема, по которой сошлись
                    { "avatarUrl", fullAvatarUrl },
                    { "chatType", ChatType } // Передаем тип чата дальше на страницу чата
                };
                // Определяем, на какую страницу чата переходить
                string targetPage = ChatType == "Текстовый чат" ? nameof(Views.TextChatPage) : nameof(Views.VoiceChatPage);
                // Пока VoiceChatPage не готова, можно всегда на TextChatPage
                // targetPage = nameof(Views.TextChatPage); 

                await Shell.Current.GoToAsync(targetPage, true, navigationParameters);
            });
        }

        private Task OnSearchStatusUpdate(string status)
        {
            _dispatcher.Dispatch(() => StatusMessage = status); 
            return Task.CompletedTask;
        }
        
        private Task OnCallEndedByPartner_NotInCall()
        {
             Debug.WriteLine("SearchingViewModel: Received OnCallEndedByPartner, but not in a call state. Ignoring.");
             return Task.CompletedTask;
        }
        [RelayCommand]
        private async Task PageDisappearing()
        {
            IsSearching = false; 
            StopTimer();
            if (string.IsNullOrEmpty(_signalRService.CurrentCallGroupId)) 
            {
                Debug.WriteLine("SearchingViewModel.PageDisappearing: Not in a call, disconnecting SignalR.");
                await _signalRService.DisconnectAsync(); 
            } else {
                Debug.WriteLine("SearchingViewModel.PageDisappearing: In a call, not disconnecting SignalR here.");
            }
        }
        private void StartTimer()
        {
            _timer?.Stop(); 
            _timer = _dispatcher.CreateTimer(); 
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }
        private void StopTimer()
        {
            _timer?.Stop();
            _timer = null;
        }
        private void OnTimerTick(object? sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            TimeElapsed = _elapsedTime.ToString(@"m\:ss");
            if (IsSearching && !FindAnyoneActivated && !IsFindAnyoneButtonVisible && _elapsedTime.TotalSeconds >= 15)
            {
                _dispatcher.Dispatch(() => 
                {
                    IsFindAnyoneButtonVisible = true;
                    StatusMessage = "Не удалось найти идеальную пару.\nПопробовать найти любого?";
                });
            }
        }
        [RelayCommand]
        private async Task FindAnyone()
        {
            if (!_signalRService.IsConnected)
            {
                StatusMessage = "Нет подключения к серверу."; 
                return;
            }
            FindAnyoneActivated = true; 
            IsFindAnyoneButtonVisible = false; 
            StatusMessage = "Расширяем критерии поиска (любой собеседник)..."; 
            await _signalRService.FindAnyoneAsync();
        }
        [RelayCommand]
        private async Task CancelSearch()
        {
            IsSearching = false; 
            await Shell.Current.GoToAsync("..", true); 
        }
    }
}