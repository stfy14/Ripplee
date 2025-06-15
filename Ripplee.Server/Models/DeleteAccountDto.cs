using System.ComponentModel.DataAnnotations;

namespace Ripplee.Server.Models
{
    public class DeleteAccountDto
    {
        [Required]
        public required string Password { get; set; }
    }
}