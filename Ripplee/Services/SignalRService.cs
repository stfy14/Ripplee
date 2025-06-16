using Microsoft.AspNetCore.SignalR.Client;
using Ripplee.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Ripplee.Services.Services
{
    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public string? CurrentCallGroupId { get; private set; }

        private Func<string, string, string, string, string?, Task>? _onCompanionFoundHandler; // Обновлен тип
        private Func<string, Task>? _onSearchStatusHandler;
        private Func<Task>? _onCallEndedByPartnerHandler;
        private Func<bool, Task>? _onPartnerMuteStatusChangedHandler; // Добавлен

        public SignalRService() { }

        public async Task ConnectAsync(
            Func<string, string, string, string, string?, Task> onCompanionFound, // Обновлен тип
            Func<string, Task> onSearchStatus,
            Func<Task> onCallEndedByPartner,
            Func<bool, Task> onPartnerMuteStatusChanged)
        {
            _onCompanionFoundHandler = onCompanionFound; // Сохраняем обновленный тип
            _onSearchStatusHandler = onSearchStatus;
            _onCallEndedByPartnerHandler = onCallEndedByPartner;
            _onPartnerMuteStatusChangedHandler = onPartnerMuteStatusChanged;

            if (IsConnected && _hubConnection != null)
            {
                Debug.WriteLine("SignalRService: Already connected. Re-registering handlers.");
                RegisterHubEventHandlers(); // Перерегистрируем на случай, если коллбэки изменились
                return;
            }

            string hubUrl = MauiProgram.GetApiBaseAdress().TrimEnd('/') + "/matchmakingHub";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options => {
                    options.AccessTokenProvider = async () => await SecureStorage.Default.GetAsync("auth_token");
                })
                .WithAutomaticReconnect()
                .Build();

            RegisterHubEventHandlers();

            _hubConnection.Closed += async (error) => {
                Debug.WriteLine($"SignalR: Connection closed. Error: {error?.Message}. CurrentCallGroupId: {CurrentCallGroupId}");
                CurrentCallGroupId = null;
            };

            try
            {
                var initialTokenCheck = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(initialTokenCheck))
                {
                    Debug.WriteLine("SignalRService: Cannot start connection, no initial auth token found for Hub.");
                    if (_onSearchStatusHandler != null) await _onSearchStatusHandler("Ошибка аутентификации для поиска.");
                    return;
                }
                await _hubConnection.StartAsync();
                Debug.WriteLine("SignalRService: Connection started successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignalRService: Error starting connection: {ex.Message}");
                if (_onSearchStatusHandler != null) await _onSearchStatusHandler($"Ошибка подключения: {ex.Message}");
            }
        }

        private void RegisterHubEventHandlers()
        {
            if (_hubConnection == null) return;

            // Обновлена сигнатура для CompanionFound
            _hubConnection.On<string, string, string, string, string?>("CompanionFound",
                async (name, city, topic, callGroupId, companionAvatarUrl) =>
                {
                    Debug.WriteLine($"SignalR: CompanionFound - Name: {name}, AvatarUrl: {companionAvatarUrl}, CallGroupId: {callGroupId}");
                    CurrentCallGroupId = callGroupId;
                    if (_onCompanionFoundHandler != null)
                        await _onCompanionFoundHandler(name, city, topic, callGroupId, companionAvatarUrl);
                });

            _hubConnection.On<string>("SearchStatus", async (status) => {
                if (_onSearchStatusHandler != null) await _onSearchStatusHandler(status);
            });

            _hubConnection.On("CallEndedByPartner", async () => {
                CurrentCallGroupId = null;
                if (_onCallEndedByPartnerHandler != null) await _onCallEndedByPartnerHandler();
            });

            _hubConnection.On<bool>("PartnerMuteStatusChanged", async (isMuted) => {
                if (_onPartnerMuteStatusChangedHandler != null) await _onPartnerMuteStatusChangedHandler(isMuted);
            });
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                Debug.WriteLine($"SignalRService: DisconnectAsync called. Current state: {_hubConnection.State}");
                CurrentCallGroupId = null;
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                Debug.WriteLine("SignalRService: Connection stopped and disposed.");
            }
            _onCompanionFoundHandler = null;
            _onSearchStatusHandler = null;
            _onCallEndedByPartnerHandler = null;
            _onPartnerMuteStatusChangedHandler = null; // Очистка
        }

        public async Task FindCompanionAsync(string userCity, string searchGender, string searchCity, string searchTopic)
        {
            if (!IsConnected || _hubConnection == null)
            {
                Debug.WriteLine("SignalRService: Not connected. Cannot FindCompanion.");
                if (_onSearchStatusHandler != null) await _onSearchStatusHandler("Нет подключения к серверу.");
                return;
            }
            try { await _hubConnection.InvokeAsync("FindCompanion", userCity, searchGender, searchCity, searchTopic); }
            catch (Exception ex) { Debug.WriteLine($"SignalRService: Error invoking FindCompanion: {ex.Message}"); }
        }

        public async Task FindAnyoneAsync()
        {
            if (!IsConnected || _hubConnection == null)
            {
                Debug.WriteLine("SignalRService: Not connected. Cannot FindAnyone.");
                if (_onSearchStatusHandler != null) await _onSearchStatusHandler("Нет подключения к серверу.");
                return;
            }
            try { await _hubConnection.InvokeAsync("FindAnyone"); }
            catch (Exception ex) { Debug.WriteLine($"SignalRService: Error invoking FindAnyone: {ex.Message}"); }
        }

        public async Task NotifyEndCallAsync()
        {
            if (!IsConnected || _hubConnection == null)
            {
                Debug.WriteLine("SignalRService: Not connected. Cannot NotifyEndCall.");
                return;
            }
            try
            {
                await _hubConnection.InvokeAsync("EndCallNotification");
                Debug.WriteLine("SignalRService: EndCallNotification invoked on server.");
                CurrentCallGroupId = null;
            }
            catch (Exception ex) { Debug.WriteLine($"SignalRService: Error invoking EndCallNotification: {ex.Message}"); }
        }

        public Task LeaveCurrentCallGroupAsync()
        {
            if (!string.IsNullOrEmpty(CurrentCallGroupId))
            {
                Debug.WriteLine($"SignalRService: Client is locally noting departure from call group {CurrentCallGroupId}.");
                CurrentCallGroupId = null;
            }
            return Task.CompletedTask;
        }

        // Новая реализация
        public async Task SendMuteStatusAsync(bool isMuted)
        {
            if (!IsConnected || _hubConnection == null || string.IsNullOrEmpty(CurrentCallGroupId))
            {
                Debug.WriteLine($"SignalRService: Not connected or not in a call (CallGroupId: {CurrentCallGroupId}). Cannot SendMuteStatus.");
                return;
            }
            try
            {
                await _hubConnection.InvokeAsync("ToggleMuteStatus", isMuted, CurrentCallGroupId);
                Debug.WriteLine($"SignalRService: ToggleMuteStatus invoked on server (isMuted: {isMuted}, callGroupId: {CurrentCallGroupId}).");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignalRService: Error invoking ToggleMuteStatus: {ex.Message}");
            }
        }
    }
}