using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class ChangeUsernameDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "New username must be between 3 and 50 characters.")]
        public required string NewUsername { get; set; }

        [Required]
        public required string CurrentPassword { get; set; }
    }
}