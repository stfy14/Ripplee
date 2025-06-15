using Microsoft.AspNetCore.SignalR.Client;
using Ripplee.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // Для SecureStorage

namespace Ripplee.Services.Services
{
    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;
        private readonly IUserService _userService; // Для получения токена

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public SignalRService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task ConnectAsync(
            Func<string, string, string, Task> onCompanionFound,
            Func<string, Task> onSearchStatus)
        {
            if (IsConnected) return;

            var token = await SecureStorage.Default.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("SignalRService: No auth token found. Cannot connect.");
                // Можно выбросить исключение или вернуть false/статус
                return;
            }

            string hubUrl = MauiProgram.GetApiBaseAdress().TrimEnd('/') + "/matchmakingHub";
            Debug.WriteLine($"SignalRService: Connecting to {hubUrl}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    // Передаем токен для аутентификации на хабе
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
                .WithAutomaticReconnect() // Автоматическое переподключение
                .Build();

            // Регистрируем обработчики сообщений от сервера
            _hubConnection.On<string, string, string>("CompanionFound", async (name, city, topic) =>
            {
                Debug.WriteLine($"SignalR: CompanionFound - Name: {name}, City: {city}, Topic: {topic}");
                await onCompanionFound(name, city, topic);
            });

            _hubConnection.On<string>("SearchStatus", async (status) =>
            {
                Debug.WriteLine($"SignalR: SearchStatus - {status}");
                await onSearchStatus(status);
            });

            _hubConnection.Closed += async (error) =>
            {
                Debug.WriteLine($"SignalR: Connection closed. Error: {error?.Message}");
                // Здесь можно добавить логику уведомления пользователя или попытку переподключения вручную,
                // хотя WithAutomaticReconnect уже должен это делать.
                await Task.Delay(new Random().Next(0, 5) * 1000); // Небольшая задержка перед авто-реконнектом
                // Попытка старта соединения, если оно не было преднамеренно остановлено
                if (_hubConnection != null && _hubConnection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await _hubConnection.StartAsync();
                        Debug.WriteLine("SignalR: Reconnected after explicit start.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"SignalR: Failed to restart connection: {ex.Message}");
                    }
                }
            };


            try
            {
                await _hubConnection.StartAsync();
                Debug.WriteLine("SignalRService: Connection started.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignalRService: Error starting connection: {ex.Message}");
                // Обработка ошибки подключения (например, показать сообщение пользователю)
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null && IsConnected)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync(); // Важно освободить ресурсы
                _hubConnection = null;
                Debug.WriteLine("SignalRService: Connection stopped and disposed.");
            }
        }

        public async Task FindCompanionAsync(string userCity, string searchGender, string searchCity, string searchTopic)
        {
            if (!IsConnected || _hubConnection == null)
            {
                Debug.WriteLine("SignalRService: Not connected. Cannot FindCompanion.");
                return;
            }
            try
            {
                // Имя пользователя и его пол сервер получит из токена (Claims)
                // userCity - это собственный город пользователя, который мы передаем
                await _hubConnection.InvokeAsync("FindCompanion", userCity, searchGender, searchCity, searchTopic);
                Debug.WriteLine($"SignalRService: FindCompanion invoked with UserCity: {userCity}, SearchGender: {searchGender}, SearchCity: {searchCity}, SearchTopic: {searchTopic}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignalRService: Error invoking FindCompanion: {ex.Message}");
            }
        }

        public async Task FindAnyoneAsync()
        {
            if (!IsConnected || _hubConnection == null)
            {
                Debug.WriteLine("SignalRService: Not connected. Cannot FindAnyone.");
                return;
            }
            try
            {
                await _hubConnection.InvokeAsync("FindAnyone");
                Debug.WriteLine("SignalRService: FindAnyone invoked.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignalRService: Error invoking FindAnyone: {ex.Message}");
            }
        }
    }
}