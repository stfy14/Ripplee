using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// using Ripplee.Services.Interfaces; // Раскомментируй, когда будет сервис SignalR

namespace Ripplee.ViewModels
{
    // Атрибуты для получения параметров навигации
    [QueryProperty(nameof(SearchGender), "gender")]
    [QueryProperty(nameof(SearchCity), "city")]
    [QueryProperty(nameof(SearchTopic), "topic")]
    public partial class SearchingViewModel : ObservableObject
    {
        // TODO: Раскомментировать, когда будет сервис SignalR
        // private readonly ISignalRService _signalRService;
        private IDispatcherTimer? _timer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        [ObservableProperty] private string searchGender = string.Empty;
        [ObservableProperty] private string searchCity = string.Empty;
        [ObservableProperty] private string searchTopic = string.Empty;

        [ObservableProperty] private string timeElapsed = "0:00";
        [ObservableProperty] private string statusMessage = "Подключаемся к серверу...";
        [ObservableProperty] private bool isFindAnyoneButtonVisible = false;

        // public SearchingViewModel(ISignalRService signalRService)
        public SearchingViewModel() // Конструктор должен быть public
        {
            // _signalRService = signalRService;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsFindAnyoneButtonVisible = false;
            _elapsedTime = TimeSpan.Zero;

            // Здесь будет логика подключения к хабу и отправки запроса
            // Например:
            // await _signalRService.ConnectAsync();
            // await _signalRService.FindCompanion(SearchGender, SearchCity, SearchTopic);

            StatusMessage = "Ищем идеального собеседника...";
            StartTimer();
            await Task.CompletedTask; // Временно, чтобы убрать warning
        }

        [RelayCommand]
        private async Task PageDisappearing()
        {
            StopTimer();
            // Здесь будет логика отключения
            // await _signalRService.DisconnectAsync();
            await Task.CompletedTask; // Временно
        }

        private void StartTimer()
        {
            _timer = Application.Current.Dispatcher.CreateTimer();
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

            if (!IsFindAnyoneButtonVisible && _elapsedTime.TotalSeconds >= 15)
            {
                IsFindAnyoneButtonVisible = true;
                StatusMessage = "Не удалось найти идеальную пару. Попробовать найти любого?";
            }
        }

        [RelayCommand]
        private async Task FindAnyone()
        {
            IsFindAnyoneButtonVisible = false;
            StatusMessage = "Расширяем критерии поиска...";
            // TODO: Вызвать новый метод на сервере
            // await _signalRService.FindAnyone();
            await Task.CompletedTask; // Временно
        }

        [RelayCommand]
        private async Task CancelSearch()
        {
            await Shell.Current.GoToAsync("..", true);
        }
    }
}