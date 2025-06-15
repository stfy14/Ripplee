namespace Ripplee.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string? MyGender { get; set; }
        public string? MyCity { get; set; }
        public string? AvatarUrl { get; set; } 
    }
}