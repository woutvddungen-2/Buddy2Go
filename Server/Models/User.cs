namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<JourneyParticipants> JourneyParticipations { get; set; } = null!;
        public ICollection<JourneyMessages> SentMessages { get; set; } = null!;
        public ICollection<DangerousPlace> Reports { get; set; } = null!;
        public ICollection<Journey> OwnedJourneys { get; set; } = null!;

    }
}
