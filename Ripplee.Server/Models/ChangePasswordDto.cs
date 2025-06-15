using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class ChangePasswordDto
    {
        [Required]
        public required string OldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")] // Добавим валидацию длины
        public required string NewPassword { get; set; }
    }
}