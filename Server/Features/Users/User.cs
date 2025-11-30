using Server.Features.Chats;
using Server.Features.DangerousPlaces;
using Server.Features.Journeys;

namespace Server.Features.Users
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
        public ICollection<JourneyParticipant> JourneyParticipations { get; set; } = null!;
        public ICollection<JourneyMessage> SentMessages { get; set; } = null!;
        public ICollection<DangerousPlace> Reports { get; set; } = null!;
        public ICollection<Journey> OwnedJourneys { get; set; } = null!;

    }
}
