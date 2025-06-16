using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class LoginUserDto
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}