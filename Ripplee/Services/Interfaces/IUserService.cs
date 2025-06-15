using Ripplee.Models;
using System.IO; // Для Stream
using System.Threading.Tasks; // Для Task

namespace Ripplee.Services.Interfaces
{
    public interface IUserService
    {
        UserModel CurrentUser { get; }
        UserStatus CurrentStatus { get; }
        Task InitializeAsync();
        Task<bool> LoginAsGuestAsync(string username);
        Task<bool> RegisterAndLoginAsync(string username, string password);
        Task<bool> LoginAsync(string username, string password);
        Task<bool> TryAutoLoginAsync();
        Task LogoutAsync();
        Task<bool> UpdateMyCriteriaAsync();
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string oldPassword, string newPassword); 
        Task<bool> UploadAvatarAsync(Stream imageData, string fileName); 
        Task<(bool Success, string? ErrorMessage)> ChangeUsernameAsync(string newUsername, string currentPassword);
        Task<(bool Success, string? ErrorMessage)> DeleteAccountAsync(string password);
    }
}