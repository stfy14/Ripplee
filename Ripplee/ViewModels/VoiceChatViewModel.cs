using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Services.Interfaces; 
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching; 

namespace Ripplee.ViewModels
{
    [QueryProperty(nameof(CompanionName), "name")]
    [QueryProperty(nameof(City), "city")]     
    [QueryProperty(nameof(Topic), "topic")]
    [QueryProperty(nameof(CompanionAvatarUrl), "avatarUrl")]
    public partial class VoiceChatViewModel : ObservableObject
    {
        private IDispatcherTimer? _callTimer;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private readonly ISignalRService _signalRService; 
        private readonly IDispatcher _dispatcher; 

        [ObservableProperty] private string? companionName;
        [ObservableProperty] private string? city; 
        [ObservableProperty] private string? topic;
        [ObservableProperty] private string? companionAvatarUrl;
        [ObservableProperty] private string callDuration = "0:00";
        [ObservableProperty] private bool isCompanionMuted = false;
        [ObservableProperty] private bool isCompanionSpeaking = false; 

        private bool _isClosing = false;
        private bool _currentUserInitiatedEndCall = false;

        private bool _isMuted = false;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (SetProperty(ref _isMuted, value))
                {
                    if (_signalRService.IsConnected && !string.IsNullOrEmpty(_signalRService.CurrentCallGroupId))
                    {
                        Task.Run(async () => await _signalRService.SendMuteStatusAsync(value));
                    }
                    Debug.WriteLine($"VoiceChatViewModel: Own Mute status changed to: {value}");
                }
            }
        }


        public VoiceChatViewModel(ISignalRService signalRService, IDispatcher dispatcher)
        {
            _signalRService = signalRService;
            _dispatcher = dispatcher;
        }

        private async Task CloseCallPage() 
        {
            if (_isClosing) return;
            _isClosing = true;
            StopCallTimer();
            await _signalRService.LeaveCurrentCallGroupAsync(); 
            await _dispatcher.DispatchAsync(async () => {
                if (Shell.Current.CurrentPage is Views.VoiceChatPage) {
                    await Shell.Current.GoToAsync("//MainPage", true);
                }
            });
        }

        private Task HandlePartnerMuteStatusChanged(bool partnerIsMuted)
        {
            Debug.WriteLine($"VoiceChatViewModel: HandlePartnerMuteStatusChanged received: {partnerIsMuted}");
            _dispatcher.Dispatch(() => IsCompanionMuted = partnerIsMuted);
            return Task.CompletedTask;
        }
        
        private async Task HandleCallEndedByPartner() 
        {
            Debug.WriteLine($"VoiceChatViewModel: HandleCallEndedByPartner called. _currentUserInitiatedEndCall: {_currentUserInitiatedEndCall}");
            if (!_currentUserInitiatedEndCall) {
                 await _dispatcher.DispatchAsync(async () => {
                    await Shell.Current.DisplayAlert("Звонок завершен", "Ваш собеседник завершил звонок.", "OK");
                 });
                await CloseCallPage();
            }
        }

        [RelayCommand]
        private void ToggleMute() 
        {
            IsMuted = !IsMuted; 
        }

        [RelayCommand]
        private async Task EndCall() 
        {
            if (_isClosing) return;
            _currentUserInitiatedEndCall = true; 
            Debug.WriteLine("VoiceChatViewModel: EndCall command executed by this user.");
            await _signalRService.NotifyEndCallAsync(); 
            await CloseCallPage(); 
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            _isClosing = false;
            _currentUserInitiatedEndCall = false;
            IsCompanionMuted = false;
            Debug.WriteLine($"VoiceChatViewModel Appearing: CompanionName={CompanionName}, AvatarUrl={CompanionAvatarUrl}");


            StartCallTimer();

            await _signalRService.ConnectAsync(
                async (n, c, t, id, avatar) => { await Task.CompletedTask; },
                async (s) => { await Task.CompletedTask; },
                HandleCallEndedByPartner,
                HandlePartnerMuteStatusChanged,
                async (sender, msg, avatarUrl) => { /* VoiceChat не обрабатывает текстовые сообщения напрямую */ await Task.CompletedTask; }

            );

            if (!_signalRService.IsConnected)
            {
                await _dispatcher.DispatchAsync(async () => {
                    await Shell.Current.DisplayAlert("Ошибка связи", "Потеряно соединение с сервером чата.", "OK");
                });
                await CloseCallPage();
            }
            else
            {
                if (!string.IsNullOrEmpty(_signalRService.CurrentCallGroupId))
                {
                    await _signalRService.SendMuteStatusAsync(IsMuted); 
                }
            }
        }

        [RelayCommand]
        private async Task PageDisappearing()
        {
            Debug.WriteLine($"VoiceChatViewModel: PageDisappearing. IsClosing: {_isClosing}, CurrentUserInitiated: {_currentUserInitiatedEndCall}");
            if (!_isClosing && !_currentUserInitiatedEndCall) {
                Debug.WriteLine("VoiceChatViewModel: Page disappearing (e.g. back nav) without explicit EndCall by this user, notifying partner.");
                await _signalRService.NotifyEndCallAsync(); 
            }
            if (!_isClosing) { 
                await CloseCallPage();
            } else { 
                 StopCallTimer();
            }
        }
        
        private void StartCallTimer()
        {
            _callTimer?.Stop();
            _callTimer = _dispatcher.CreateTimer(); 
            _callTimer.Interval = TimeSpan.FromSeconds(1);
            _callTimer.Tick += OnTimerTick;
            _callTimer.Start();
        }

        private void StopCallTimer()
        {
            _callTimer?.Stop();
            _callTimer = null;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            CallDuration = _elapsedTime.ToString(@"m\:ss");
            int seconds = _elapsedTime.Seconds % 10;

            if (!IsCompanionMuted) 
            {
                IsCompanionSpeaking = seconds is > 5 and < 8; 
            } else {
                IsCompanionSpeaking = false; 
            }
        }
    }
}