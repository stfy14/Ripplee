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

        private async Task<bool> LoadProfileAsync()
        {
            var profile = await _apiClient.GetProfileAsync();
            if (profile == null)
            {
                await LogoutAsync();
                return false;
            }

            CurrentUser = new UserModel
            {
                Username = profile.Username ?? "Unknown",
                MyGender = profile.MyGender ?? "Не указан",
                MyCity = profile.MyCity ?? "Не указан"
            };

            if (!string.IsNullOrEmpty(profile.AvatarUrl))
            {
                string baseAddress = MauiProgram.GetApiBaseAdress();
                CurrentUser.AvatarUrl = baseAddress.TrimEnd('/') + profile.AvatarUrl;
            }
            else
            {
                CurrentUser.AvatarUrl = null;
            }

            CurrentStatus = UserStatus.Registered;
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));
            Debug.WriteLine($"Profile loaded for {CurrentUser.Username}. Avatar: {CurrentUser.AvatarUrl}");
            return true;
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return false;
            return await LoadProfileAsync();
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var token = await _apiClient.LoginAsync(username, password);
            if (string.IsNullOrEmpty(token)) return false;
            await SecureStorage.Default.SetAsync("auth_token", token);
            return await LoadProfileAsync();
        }

        public async Task<bool> RegisterAndLoginAsync(string username, string password)
        {
            bool registrationSuccess = await _apiClient.RegisterAsync(username, password);
            if (!registrationSuccess) return false;
            return await LoginAsync(username, password);
        }

        public async Task<bool> LoginAsGuestAsync(string username)
        {
            await Task.Delay(500);
            CurrentUser = new UserModel { Username = username };
            CurrentStatus = UserStatus.Guest;
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));
            return true;
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("auth_token");
            CurrentUser = new();
            CurrentStatus = UserStatus.Unauthenticated;
            WeakReferenceMessenger.Default.Send(new UserChangedMessage(null));
            await Task.CompletedTask;
        }

        public async Task<bool> UpdateMyCriteriaAsync()
        {
            if (CurrentStatus != UserStatus.Registered) return false;

            var (success, newToken) = await _apiClient.UpdateUserCriteriaAsync(CurrentUser.MyGender, CurrentUser.MyCity);
            if (success && !string.IsNullOrEmpty(newToken))
            {
                await SecureStorage.Default.SetAsync("auth_token", newToken); 
                Debug.WriteLine("UserService: Criteria updated and new token saved.");
            }
            return success;
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            if (CurrentStatus != UserStatus.Registered) return (false, "User not registered.");
            return await _apiClient.ChangePasswordOnServerAsync(oldPassword, newPassword);
        }

        public async Task<bool> UploadAvatarAsync(Stream imageData, string fileName)
        {
            if (CurrentStatus != UserStatus.Registered) return false;
            var newAvatarRelativeUrl = await _apiClient.UploadAvatarToServerAsync(imageData, fileName);
            if (!string.IsNullOrEmpty(newAvatarRelativeUrl))
            {
                string baseAddress = MauiProgram.GetApiBaseAdress();
                CurrentUser.AvatarUrl = baseAddress.TrimEnd('/') + newAvatarRelativeUrl + $"?v={Guid.NewGuid()}"; // Cache buster
                WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));
                return true;
            }
            return false;
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangeUsernameAsync(string newUsername, string currentPassword)
        {
            if (CurrentStatus != UserStatus.Registered) return (false, "User not registered.");
            var (newToken, serverMessage) = await _apiClient.ChangeUsernameOnServerAsync(newUsername, currentPassword);
            if (!string.IsNullOrEmpty(newToken))
            {
                await SecureStorage.Default.SetAsync("auth_token", newToken);
                CurrentUser.Username = newUsername;
                WeakReferenceMessenger.Default.Send(new UserChangedMessage(CurrentUser));
                return (true, serverMessage ?? "Username changed.");
            }
            return (false, serverMessage ?? "Failed to change username.");
        }
        public async Task<(bool Success, string? ErrorMessage)> DeleteAccountAsync(string password)
        {
            if (CurrentStatus != UserStatus.Registered)
            {
                return (false, "User not registered or is a guest.");
            }
            var (success, errorMessage) = await _apiClient.DeleteAccountOnServerAsync(password);
            if (success)
            {
                await LogoutAsync(); // Разлогиниваем пользователя после успешного удаления
            }
            return (success, errorMessage);
        }
    }
}