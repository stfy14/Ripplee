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

        // Храним ссылки на коллбэки
        private Func<string, string, string, string, string?, Task>? _onCompanionFoundHandler;
        private Func<string, Task>? _onSearchStatusHandler;
        private Func<Task>? _onCallEndedByPartnerHandler;
        private Func<bool, Task>? _onPartnerMuteStatusChangedHandler;
        private Func<string, string, string?, Task>? _onReceiveTextMessageHandler;

        // Храним IDisposable для отписки от событий
        private IDisposable? _companionFoundSubscription;
        private IDisposable? _searchStatusSubscription;
        private IDisposable? _callEndedSubscription;
        private IDisposable? _partnerMuteSubscription;
        private IDisposable? _receiveTextSubscription; // <--- Для текстовых сообщений

        public async Task ConnectAsync(
                Func<string, string, string, string, string?, Task> onCompanionFound,
                Func<string, Task> onSearchStatus,
                Func<Task> onCallEndedByPartner,
                Func<bool, Task> onPartnerMuteStatusChanged,
                Func<string, string, string?, Task> onReceiveTextMessage)
        {
            // Сохраняем новые коллбэки
            _onCompanionFoundHandler = onCompanionFound;
            _onSearchStatusHandler = onSearchStatus;
            _onCallEndedByPartnerHandler = onCallEndedByPartner;
            _onPartnerMuteStatusChangedHandler = onPartnerMuteStatusChanged;
            _onReceiveTextMessageHandler = onReceiveTextMessage;

            if (_hubConnection == null)
            {
                string hubUrl = MauiProgram.GetApiBaseAdress().TrimEnd('/') + "/matchmakingHub";
                Debug.WriteLine($"SignalRService: Creating new HubConnection to {hubUrl}");
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options => {
                        options.AccessTokenProvider = async () => await SecureStorage.Default.GetAsync("auth_token");
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Регистрируем обработчики один раз при создании _hubConnection
                // и при каждой смене коллбэков (что происходит при этом вызове ConnectAsync)
                RegisterHubEventHandlers();

                _hubConnection.Closed += async (error) => {
                    Debug.WriteLine($"SignalR: Connection closed. Error: {error?.Message}. CurrentCallGroupId: {CurrentCallGroupId}");
                    CurrentCallGroupId = null;
                    UnsubscribeAllEvents(); // Отписываемся от всего при закрытии
                };
            }
            else
            {
                // Соединение уже существует, просто перерегистрируем/обновляем обработчики
                Debug.WriteLine("SignalRService: HubConnection exists. Re-registering handlers.");
                RegisterHubEventHandlers();
            }

            // Пытаемся подключиться, если еще не подключены
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                Debug.WriteLine("SignalRService: HubConnection is disconnected. Attempting to start...");
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
            else
            {
                Debug.WriteLine($"SignalRService: HubConnection state is already: {_hubConnection.State}. Handlers re-registered if needed.");
            }
        }

        private void UnsubscribeAllEvents()
        {
            _companionFoundSubscription?.Dispose();
            _searchStatusSubscription?.Dispose();
            _callEndedSubscription?.Dispose();
            _partnerMuteSubscription?.Dispose();
            _receiveTextSubscription?.Dispose();

            _companionFoundSubscription = null;
            _searchStatusSubscription = null;
            _callEndedSubscription = null;
            _partnerMuteSubscription = null;
            _receiveTextSubscription = null;
            Debug.WriteLine("SignalRService: All event subscriptions disposed.");
        }

        private void RegisterHubEventHandlers()
        {
            if (_hubConnection == null) return;

            // Отписываемся от предыдущих перед новой подпиской
            UnsubscribeAllEvents();

            Debug.WriteLine("SignalRService: Registering new event handlers.");

            _companionFoundSubscription = _hubConnection.On<string, string, string, string, string?>("CompanionFound",
                async (name, city, topic, callGroupId, companionAvatarUrl) => {
                    CurrentCallGroupId = callGroupId;
                    if (_onCompanionFoundHandler != null) await _onCompanionFoundHandler(name, city, topic, callGroupId, companionAvatarUrl);
                });

            _searchStatusSubscription = _hubConnection.On<string>("SearchStatus", async (status) => {
                if (_onSearchStatusHandler != null) await _onSearchStatusHandler(status);
            });

            _callEndedSubscription = _hubConnection.On("CallEndedByPartner", async () => {
                CurrentCallGroupId = null;
                if (_onCallEndedByPartnerHandler != null) await _onCallEndedByPartnerHandler();
            });

            _partnerMuteSubscription = _hubConnection.On<bool>("PartnerMuteStatusChanged", async (isMuted) => {
                if (_onPartnerMuteStatusChangedHandler != null) await _onPartnerMuteStatusChangedHandler(isMuted);
            });

            _receiveTextSubscription = _hubConnection.On<string, string, string?>("ReceiveTextMessage",
                async (senderUsername, messageText, senderAvatarUrl) => {
                    Debug.WriteLine($"SignalR: (RegisterHubEventHandlers) Received ReceiveTextMessage from {senderUsername}: {messageText}");
                    if (_onReceiveTextMessageHandler != null)
                        await _onReceiveTextMessageHandler(senderUsername, messageText, senderAvatarUrl);
                });
        }

        public async Task FindCompanionAsync(string userCity, string userGender, string searchGender, string searchCity, string searchTopic, string chatType)
        {
            if (!IsConnected || _hubConnection == null)
            {
                Debug.WriteLine("SignalRService: Not connected. Cannot FindCompanion.");
                if (_onSearchStatusHandler != null) await _onSearchStatusHandler("Нет подключения к серверу.");
                return;
            }
            try
            {
                await _hubConnection.InvokeAsync("FindCompanion", userCity, userGender, searchGender, searchCity, searchTopic, chatType);
                Debug.WriteLine($"SignalRService: FindCompanion invoked with UserCity: {userCity}, UserGender: {userGender}, SearchGender: {searchGender}, SearchCity: {searchCity}, SearchTopic: {searchTopic}, ChatType: {chatType}");
            }
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

        public async Task SendTextMessageAsync(string messageText, string senderAvatarUrl) 
        {
            if (!IsConnected || _hubConnection == null || string.IsNullOrEmpty(CurrentCallGroupId))
            {
                Debug.WriteLine($"SignalRService: Not connected or not in a call (CallGroupId: {CurrentCallGroupId}). Cannot SendTextMessage.");
                return;
            }
            try
            {
                await _hubConnection.InvokeAsync("SendTextMessage", CurrentCallGroupId, messageText, senderAvatarUrl);
                Debug.WriteLine($"SignalRService: SendTextMessage invoked on server for group {CurrentCallGroupId}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignalRService: Error invoking SendTextMessage: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                var currentState = _hubConnection.State;
                Debug.WriteLine($"SignalRService: DisconnectAsync called. Current state: {currentState}");

                UnsubscribeAllEvents(); // Отписываемся от событий
                CurrentCallGroupId = null;

                if (currentState != HubConnectionState.Disconnected)
                {
                    try { await _hubConnection.StopAsync(); Debug.WriteLine("SignalRService: Connection stopped."); }
                    catch (Exception ex) { Debug.WriteLine($"SignalRService: Error during StopAsync: {ex.Message}"); }
                }

                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                Debug.WriteLine("SignalRService: Connection disposed and set to null.");
            }
            // Очищаем ссылки на коллбэки
            _onCompanionFoundHandler = null;
            _onSearchStatusHandler = null;
            _onCallEndedByPartnerHandler = null;
            _onPartnerMuteStatusChangedHandler = null;
            _onReceiveTextMessageHandler = null;
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
    }
}