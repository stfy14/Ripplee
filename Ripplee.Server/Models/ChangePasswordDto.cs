using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class ChangePasswordDto
    {
        [Required]
        public required string OldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Пароль должен быть из 6 символов и больше")] 
        public required string NewPassword { get; set; }
    }
}