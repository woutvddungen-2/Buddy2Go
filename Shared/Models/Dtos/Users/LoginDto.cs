using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Users
{
    public class LoginDto
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
