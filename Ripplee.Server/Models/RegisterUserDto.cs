// Ripplee.Server/Models/RegisterUserDto.cs
using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class RegisterUserDto
    {
        [Required] // Говорит, что поле обязательное
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}