using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Models;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using System.Diagnostics;

namespace Ripplee.Services.Services
{
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

        public Task InitializeAsync()
        {
            Debug.WriteLine("User service initialized.");
            return Task.CompletedTask;
        }

        // Централизованный метод загрузки профиля
        private async Task<bool> LoadProfileAsync()
        {
            var profile = await _apiClient.GetProfileAsync();
            if (profile == null)
            {
                // Если профиль не загрузился, значит токен невалидный, выходим
                await LogoutAsync();
                return false;
            }

            // Обновляем текущего пользователя данными с сервера
            CurrentUser = new UserModel
            {
                Username = profile.Username ?? "Unknown",
                // Загружаем сохраненные критерии. Если их нет (null), ставим значения по умолчанию.
                MyGender = profile.MyGender ?? "Мужчина",
                MyCity = profile.MyCity ?? "Москва"
            };
            CurrentStatus = UserStatus.Registered;

            // Оповещаем остальное приложение, что пользователь загружен
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));
            Debug.WriteLine($"Profile loaded for {CurrentUser.Username}. Gender: {CurrentUser.MyGender}, City: {CurrentUser.MyCity}");
            return true;
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // Пытаемся загрузить профиль с имеющимся токеном
            return await LoadProfileAsync();
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var token = await _apiClient.LoginAsync(username, password);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // Сохраняем новый токен и загружаем профиль
            await SecureStorage.Default.SetAsync("auth_token", token);
            return await LoadProfileAsync();
        }

        public async Task<bool> RegisterAndLoginAsync(string username, string password)
        {
            bool registrationSuccess = await _apiClient.RegisterAsync(username, password);
            if (!registrationSuccess)
            {
                Debug.WriteLine($"Registration failed for user: {username}");
                return false;
            }
            // Если регистрация прошла, сразу логинимся
            return await LoginAsync(username, password);

        }

        public async Task<bool> LoginAsGuestAsync(string username)
        {
            await Task.Delay(500); // Имитация
            CurrentUser = new UserModel { Username = username };
            CurrentStatus = UserStatus.Guest;
            Debug.WriteLine($"Logged in as GUEST: {username}");
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));
            return true;
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("auth_token");
            CurrentUser = new();
            CurrentStatus = UserStatus.Unauthenticated;
            Debug.WriteLine("User logged out and token removed.");
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(null));
            await Task.CompletedTask;
        }

        // Реализация нового метода для сохранения критериев
        public async Task<bool> UpdateMyCriteriaAsync()
        {
            if (CurrentStatus != UserStatus.Registered)
            {
                Debug.WriteLine("Cannot update criteria for guest or unauthenticated user.");
                return false;
            }

            Debug.WriteLine($"Saving criteria for {CurrentUser.Username}: Gender={CurrentUser.MyGender}, City={CurrentUser.MyCity}");
            return await _apiClient.UpdateUserCriteriaAsync(CurrentUser.MyGender, CurrentUser.MyCity);
        }
    }
}