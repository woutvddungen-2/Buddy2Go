using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.Users
{
    public class VerifyUserDto
    {
        [Required(ErrorMessage = "Telefoonnummer is verplicht")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Code is verplicht")]
        public string Code { get; set; } = string.Empty;
    }
}
