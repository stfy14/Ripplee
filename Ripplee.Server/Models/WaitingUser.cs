namespace Ripplee.Server.Models
{
    public class WaitingUser
    {
        public required string ConnectionId { get; set; }
        public required string Username { get; set; }
        public required string UserGender { get; set; }
        public required string UserCity { get; set; }
        public string? UserAvatarUrl { get; set; } 

        public required string SearchGender { get; set; }
        public required string SearchCity { get; set; }
        public required string SearchTopic { get; set; }
    }
}