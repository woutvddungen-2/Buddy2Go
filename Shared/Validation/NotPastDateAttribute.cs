using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Validation
{
    public class NotPastDateAttribute : ValidationAttribute
    {
        private readonly int _minuteBuffer;

        public NotPastDateAttribute(int minuteBuffer = 2)
        {
            _minuteBuffer = minuteBuffer;
            ErrorMessage = $"De vertrektijd mag niet in het verleden liggen.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dateTime)
                return ValidationResult.Success;

            // Allow 5-minute user buffer
            var now = DateTime.UtcNow.AddMinutes(-_minuteBuffer);

            if (dateTime.ToUniversalTime() < now)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}

