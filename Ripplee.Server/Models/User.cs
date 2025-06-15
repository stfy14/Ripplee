// Ripplee.Server/Models/User.cs
namespace Ripplee.Server.Models
{
    public class User
    {
        public int Id { get; set; } // Первичный ключ, будет генерироваться автоматически
        public required string Username { get; set; }
        public required string PasswordHash { get; set; } // Будем хранить только ХЕШ пароля
        public string? MyGender { get; set; }
        public string? MyCity { get; set; }
    }
}