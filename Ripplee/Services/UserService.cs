using Ripplee.Models;
using Ripplee.Services.Interfaces;
using System.Diagnostics;

namespace Ripplee.Services.Services
{
    public class UserService : IUserService
    {
        // Возвращаем private set
        public UserModel CurrentUser { get; private set; } = new();
        public UserStatus CurrentStatus { get; private set; } = UserStatus.Unauthenticated;

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
            Debug.WriteLine("User service initialized.");
        }

        public async Task<bool> LoginAsGuestAsync(string username)
        {
            await Task.Delay(500);
            // Возвращаем создание НОВОГО объекта
            CurrentUser = new UserModel { Username = username };
            CurrentStatus = UserStatus.Guest;
            Debug.WriteLine($"Logged in as GUEST: {username}");
            return true;
        }

        // Убедись, что сигнатура совпадает с интерфейсом (topic необязательный)
        public async Task<bool> RegisterAndLoginAsync(string username, string password, string? topic = null)
        {
            await Task.Delay(1000);
            // Возвращаем создание НОВОГО объекта
            CurrentUser = new UserModel { Username = username, TopicSelection = topic ?? string.Empty };
            CurrentStatus = UserStatus.Registered;
            Debug.WriteLine($"Registered and logged in as USER: {username}");
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