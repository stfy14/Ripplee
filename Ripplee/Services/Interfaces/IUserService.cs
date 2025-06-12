using Ripplee.Models; 

namespace Ripplee.Services.Interfaces
{
    public interface IUserService
    {
        // Возвращаем private set
        UserModel CurrentUser { get; }
        UserStatus CurrentStatus { get; }
        Task InitializeAsync();
        Task<bool> LoginAsGuestAsync(string username);
        Task<bool> RegisterAndLoginAsync(string username, string password); 
        Task<bool> LoginAsync(string username, string password);
        Task<bool> TryAutoLoginAsync();
        Task LogoutAsync();
    }
}