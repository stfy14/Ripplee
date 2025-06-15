using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ripplee.ViewModels
{
    [QueryProperty(nameof(CompanionName), "name")]
    [QueryProperty(nameof(City), "city")]     
    [QueryProperty(nameof(Topic), "topic")]   
    public partial class VoiceChatViewModel : ObservableObject
    {
        private IDispatcherTimer? _callTimer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;

        [ObservableProperty]
        private string? companionName;

        [ObservableProperty]
        private string? city; 

        [ObservableProperty]
        private string? topic; 

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
            _callTimer = Application.Current.Dispatcher.CreateTimer();
            _callTimer.Interval = TimeSpan.FromSeconds(1);
            _callTimer.Tick += OnTimerTick;
            _callTimer.Start();
        }

        private void StopCallTimer()
        {
            if (_callTimer is not null)
            {
                _callTimer.Stop();
                _callTimer.Tick -= OnTimerTick;
                _callTimer = null;
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            CallDuration = _elapsedTime.ToString(@"m\:ss");

            // --- ЗАГЛУШКА для индикатора звука ---
            int seconds = _elapsedTime.Seconds % 10;
            IsCompanionSpeaking = seconds is > 5 and < 8;
        }

        [RelayCommand]
        private void ToggleMute()
        {
            IsMuted = !IsMuted;
            // TODO: В будущем здесь будет реальная логика отключения микрофона
        }

        [RelayCommand]
        private async Task EndCall()
        {
            StopCallTimer();
            await Shell.Current.GoToAsync("//MainPage", true);
        }

        [RelayCommand]
        private void PageAppearing()
        {
            StartCallTimer();
        }

        [RelayCommand]
        private void PageDisappearing()
        {
            StopCallTimer();
        }
    }
}