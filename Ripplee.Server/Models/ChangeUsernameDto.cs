using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class ChangeUsernameDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин дожнен содержать от 3 до 50 символов")]
        public required string NewUsername { get; set; }

        [Required]
        public required string CurrentPassword { get; set; }
    }
}