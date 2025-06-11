using Ripplee.Models;
using Ripplee.Services.Interfaces;
using System.Diagnostics;
using Ripplee.Services.Data;

namespace Ripplee.Services.Services
{
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

        public async Task<bool> LoginAsGuestAsync(string username)
        {
            await Task.Delay(50);
            CurrentUser = new UserModel { Username = username };
            CurrentStatus = UserStatus.Guest;
            Debug.WriteLine($"Logged in as GUEST: {username}");
            return true;
        }

        // Убедись, что сигнатура совпадает с интерфейсом (topic необязательный)
        public async Task<bool> RegisterAndLoginAsync(string username, string password, string? topic = null)
        {
            bool registrationSuccess = await _apiClient.RegisterAsync(username, password);
            if (!registrationSuccess)
            {
                Debug.WriteLine($"Registration failed for user: {username}");
                // TODO: Показать ошибку пользователю
                return false;
            }

            var token = await _apiClient.LoginAsync(username, password);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            await SecureStorage.Default.SetAsync("auth_token", token);

            // ✅ НАЧАЛО БЛОКА ИЗМЕНЕНИЙ
            // Сразу после получения токена, загружаем данные профиля с сервера
            var profile = await _apiClient.GetProfileAsync();
            if (profile == null)
            {
                // Что-то пошло не так, хотя мы только что залогинились.
                // Этого не должно произойти, но лучше обработать.
                Debug.WriteLine("Failed to get profile after login.");
                return false; // Считаем вход неудачным, если не смогли получить профиль
            }

            // Заполняем модель реальными данными с сервера
            CurrentUser = new UserModel { Username = profile.Username ?? "Unknown" };
            // В будущем здесь можно будет добавить Id = profile.Id и т.д.
            CurrentStatus = UserStatus.Registered;
            Debug.WriteLine($"Logged in and profile loaded for USER: {CurrentUser.Username}");
            // ✅ КОНЕЦ БЛОКА ИЗМЕНЕНИЙ

            return true;
        }

        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
            // При выходе тоже создаем новый пустой объект
            CurrentUser = new();
            CurrentStatus = UserStatus.Unauthenticated;
            Debug.WriteLine("User logged out.");
        }
    }
}