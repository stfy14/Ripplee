// Файл: Ripplee/Services/Services/UserService.cs

using CommunityToolkit.Mvvm.Messaging; // <-- Добавлен using
using Ripplee.Models;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using System.Diagnostics;

namespace Ripplee.Services.Services
{
    // ✅ ОБЪЯВЛЯЕМ КЛАСС СООБЩЕНИЯ
    // Его можно объявить здесь или в отдельном файле.
    // Для простоты оставим здесь.
    public sealed class UserChangedMessage
    {
        public UserModel? NewUser { get; }
        public UserChangedMessage(UserModel? user)
        {
            NewUser = user;
        }
    }

    public class UserService : IUserService
    {
        private readonly ChatApiClient _apiClient;

        public UserModel CurrentUser { get; private set; } = new();
        public UserStatus CurrentStatus { get; private set; } = UserStatus.Unauthenticated;

        public UserService(ChatApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
            Debug.WriteLine("User service initialized.");
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var profile = await _apiClient.GetProfileAsync();
            if (profile == null)
            {
                SecureStorage.Default.Remove("auth_token");
                return false;
            }

            CurrentUser = new UserModel { Username = profile.Username ?? "Unknown" };
            CurrentStatus = UserStatus.Registered;
            Debug.WriteLine($"Auto-login successful for user: {CurrentUser.Username}");

            // Отправляем сообщение об успешном авто-логине
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));

            return true;
        }

        public async Task<bool> LoginAsGuestAsync(string username)
        {
            await Task.Delay(500);
            CurrentUser = new UserModel { Username = username };
            CurrentStatus = UserStatus.Guest;
            Debug.WriteLine($"Logged in as GUEST: {username}");

            // Отправляем сообщение
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));

            return true;
        }

        public async Task<bool> RegisterAndLoginAsync(string username, string password, string? topic = null)
        {
            bool registrationSuccess = await _apiClient.RegisterAsync(username, password);

            if (!registrationSuccess)
            {
                Debug.WriteLine($"Registration failed for user: {username}");
                return false;
            }

            // Переиспользуем наш метод логина, который уже отправляет сообщение
            return await LoginAsync(username, password);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var token = await _apiClient.LoginAsync(username, password);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            await SecureStorage.Default.SetAsync("auth_token", token);
            var profile = await _apiClient.GetProfileAsync();
            if (profile == null)
            {
                return false;
            }

            CurrentUser = new UserModel { Username = profile.Username ?? "Unknown" };
            CurrentStatus = UserStatus.Registered;
            Debug.WriteLine($"Logged in and profile loaded for USER: {CurrentUser.Username}");

            // ✅ ОТПРАВЛЯЕМ СООБЩЕНИЕ ОБ УСПЕШНОМ ВХОДЕ
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));

            return true;
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("auth_token");
            CurrentUser = new();
            CurrentStatus = UserStatus.Unauthenticated;
            Debug.WriteLine("User logged out and token removed.");

            // ✅ ОТПРАВЛЯЕМ СООБЩЕНИЕ О ВЫХОДЕ (с null)
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(null));

            await Task.CompletedTask;
        }
    }
}