using Ripplee.Models;    
using Ripplee.Services.Interfaces; 
public class UserService : IUserService
{
    // Здесь будет логика загрузки пользователя (пока можно оставить заглушку)
    public UserModel CurrentUser { get; private set; } = new UserModel { Username = "Алексей" };

    public async Task LoadUserAsync()
    {
        // В будущем здесь будет запрос к API для получения данных
        await Task.CompletedTask;
    }
}