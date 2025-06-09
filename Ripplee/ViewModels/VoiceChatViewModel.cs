using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    // Атрибут QueryProperty позволяет нам принимать данные при навигации
    [QueryProperty(nameof(CompanionName), "name")]
    [QueryProperty(nameof(City), "city")]     // <-- НОВЫЙ
    [QueryProperty(nameof(Topic), "topic")]   // <-- НОВЫЙ
    public partial class VoiceChatViewModel : ObservableObject
    {
        private IDispatcherTimer? _callTimer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        [ObservableProperty]
        private string? companionName;

        [ObservableProperty]
        private string? city; // <-- НОВОЕ СВОЙСТВО

        [ObservableProperty]
        private string? topic; // <-- НОВОЕ СВОЙСТВО

        [ObservableProperty]
        private string callDuration = "0:00";

        [ObservableProperty]
        private bool isMuted = false;

        [ObservableProperty]
        private bool isCompanionSpeaking = false;

        public VoiceChatViewModel()
        {
            // Здесь можно инициализировать что-то, если нужно
        }

        private void StartCallTimer()
        {
            // Используем IDispatcherTimer, т.к. он работает в потоке UI и безопасен для обновления интерфейса
            _callTimer = Application.Current.Dispatcher.CreateTimer();
            _callTimer.Interval = TimeSpan.FromSeconds(1);
            _callTimer.Tick += OnTimerTick;
            _callTimer.Start();
            Debug.WriteLine("Таймер звонка запущен.");
        }

        private void StopCallTimer()
        {
            if (_callTimer is not null)
            {
                _callTimer.Stop();
                _callTimer.Tick -= OnTimerTick;
                _callTimer = null;
                Debug.WriteLine("Таймер звонка остановлен.");
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            CallDuration = _elapsedTime.ToString(@"m\:ss");

            // --- ЗАГЛУШКА для индикатора звука ---
            // Имитируем, что собеседник говорит каждые 5-8 секунд по 2 секунды
            int seconds = _elapsedTime.Seconds % 10;
            IsCompanionSpeaking = seconds is > 5 and < 8;
        }

        [RelayCommand]
        private void ToggleMute()
        {
            IsMuted = !IsMuted;
            // TODO: В будущем здесь будет реальная логика отключения микрофона
            Debug.WriteLine($"Микрофон отключен: {IsMuted}");
        }

        [RelayCommand]
        private async Task EndCall()
        {
            StopCallTimer();
            // Возвращаемся на предыдущую страницу
            await Shell.Current.GoToAsync("..", true);
        }

        // Команды для управления жизненным циклом страницы (запуск/остановка таймера)
        [RelayCommand]
        private void PageAppearing()
        {
            StartCallTimer();
        }

        [RelayCommand]
        private void PageDisappearing()
        {
            // Убедимся, что таймер остановлен, если пользователь свернул приложение или ушел со страницы
            StopCallTimer();
        }
    }
}