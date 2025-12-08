using System.ComponentModel.DataAnnotations;

namespace Shared.Validation
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? password = value as string;

            if (string.IsNullOrWhiteSpace(password))
                return new ValidationResult("Wachtwoord is verplicht.");

            if (password.Length < 8)
                return new ValidationResult("Wachtwoord moet minstens 8 tekens bevatten.");

            if (!password.Any(char.IsUpper))
                return new ValidationResult("Wachtwoord moet minstens één hoofdletter bevatten.");

            if (!password.Any(char.IsLower))
                return new ValidationResult("Wachtwoord moet minstens één kleine letter bevatten.");

            if (!password.Any(char.IsDigit))
                return new ValidationResult("Wachtwoord moet minstens één cijfer bevatten.");

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return new ValidationResult("Wachtwoord moet minstens één speciaal teken bevatten.");

            return ValidationResult.Success;
        }
    }
}
