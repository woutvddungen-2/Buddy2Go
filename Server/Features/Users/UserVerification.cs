using Shared.Models.Enums;

namespace Server.Features.Users
{
    public class UserVerification
    {
        public int Id { get; set; }
        public int? UserId { get; set; } = null; //null for registration
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public VerificationType Type { get; set; }
        public DateTime ExpiresAt { get; set; }

        // --- Temporary data ONLY for registration ---
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
    }
}
