using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Services.Interfaces; // <--- ДОБАВЛЕНО
using System.Diagnostics;       // <--- ДОБАВЛЕНО
using System.Threading.Tasks;   // <--- ДОБАВЛЕНО

namespace Ripplee.ViewModels
{
    [QueryProperty(nameof(SearchGender), "gender")]
    [QueryProperty(nameof(SearchCity), "city")]
    [QueryProperty(nameof(SearchTopic), "topic")]
    [QueryProperty(nameof(UserCity), "userCity")] // Город текущего пользователя
    public partial class SearchingViewModel : ObservableObject
    {
        private readonly ISignalRService _signalRService;
        private readonly IUserService _userService; // Для получения города пользователя, если не передан
        private IDispatcherTimer? _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        [ObservableProperty] private string searchGender = string.Empty;
        [ObservableProperty] private string searchCity = string.Empty;
        [ObservableProperty] private string searchTopic = string.Empty;
        [ObservableProperty] private string userCity = string.Empty; // Город текущего пользователя

        [ObservableProperty] private string timeElapsed = "0:00";
        [ObservableProperty] private string statusMessage = "Подключаемся к серверу...";
        [ObservableProperty] private bool isFindAnyoneButtonVisible = false;
        [ObservableProperty] private bool isSearching = false; // Флаг, что идет активный поиск
        [ObservableProperty] private bool findAnyoneActivated = false; 

        public SearchingViewModel(ISignalRService signalRService, IUserService userService)
        {
            _signalRService = signalRService;
            _userService = userService;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsSearching = true; // Начинаем поиск
            FindAnyoneActivated = false;
            IsFindAnyoneButtonVisible = false;
            _elapsedTime = TimeSpan.Zero;
            TimeElapsed = "0:00"; // Сброс таймера
            StatusMessage = "Подключение к серверу...";

            // Если userCity не был передан через навигацию, берем его из UserService
            if (string.IsNullOrEmpty(UserCity))
            {
                UserCity = _userService.CurrentUser.MyCity;
                if (string.IsNullOrEmpty(UserCity)) // На всякий случай, если и там пусто
                {
                    Debug.WriteLine("SearchingViewModel: UserCity is not set!");
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось определить ваш город. Пожалуйста, укажите его в профиле.", "OK");
                    await CancelSearch(); // Отменяем поиск, если город неизвестен
                    return;
                }
            }

            // Передаем методы-колбэки в SignalRService
            await _signalRService.ConnectAsync(OnCompanionFound, OnSearchStatusUpdate);

            if (_signalRService.IsConnected)
            {
                StatusMessage = "Ищем идеального собеседника...";
                // Отправляем запрос на поиск
                await _signalRService.FindCompanionAsync(UserCity, SearchGender, SearchCity, SearchTopic);
                StartTimer();
            }
            else
            {
                StatusMessage = "Не удалось подключиться к серверу поиска.";
                await Shell.Current.DisplayAlert("Ошибка подключения", "Не удалось подключиться к серверу для поиска собеседника. Пожалуйста, проверьте ваше интернет-соединение и попробуйте позже.", "OK");
                IsSearching = false; // Поиск не удался
            }
        }

        [RelayCommand]
        private async Task PageDisappearing()
        {
            IsSearching = false; // Завершаем поиск при уходе со страницы
            StopTimer();
            await _signalRService.DisconnectAsync();
        }

        private void StartTimer()
        {
            _timer?.Stop(); // Убедимся, что предыдущий таймер остановлен
            _timer = Application.Current!.Dispatcher.CreateTimer(); // Используем Application.Current.Dispatcher
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer?.Stop();
            // _timer.Tick -= OnTimerTick; // Отписываемся от события Tick, если таймер не будет пересоздаваться
            _timer = null;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            TimeElapsed = _elapsedTime.ToString(@"m\:ss");

            if (IsSearching && !FindAnyoneActivated && !IsFindAnyoneButtonVisible && _elapsedTime.TotalSeconds >= 15)
            {
                MainThread.BeginInvokeOnMainThread(() =>
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
                await Shell.Current.DisplayAlert("Ошибка", "Нет подключения к серверу поиска.", "OK");
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
            IsSearching = false; // Явно останавливаем поиск
            await Shell.Current.GoToAsync("..", true); // true для анимации
        }

        // --- Методы-колбэки для SignalR ---
        private async Task OnCompanionFound(string name, string city, string topic)
        {
            IsSearching = false; // Поиск завершен успешно
            StopTimer(); // Останавливаем таймер

            Debug.WriteLine($"Companion found: {name} from {city} on topic {topic}");

            // Важно: навигацию и показ DisplayAlert делать из основного потока
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // await Shell.Current.DisplayAlert("Собеседник найден!", $"Найден: {name}\nГород: {city}\nТема: {topic}", "Начать чат");

                var navigationParameters = new Dictionary<string, object>
                {
                    { "name", name },
                    { "city", city }, // Город собеседника
                    { "topic", topic } // Тема, по которой нашлись
                };
                // Переходим на страницу голосового чата
                // Убедись, что VoiceChatPage зарегистрирован для навигации
                await Shell.Current.GoToAsync(nameof(Views.VoiceChatPage), true, navigationParameters);
            });
        }

        private Task OnSearchStatusUpdate(string status)
        {
            // Обновляем сообщение статуса на UI
            MainThread.BeginInvokeOnMainThread(() => // Обновление UI из основного потока
            {
                StatusMessage = status;
            });
            return Task.CompletedTask;
        }
    }
}