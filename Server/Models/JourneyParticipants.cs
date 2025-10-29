using Shared.Models;

namespace Server.Models
{
    public class JourneyParticipants
    {
        public int JourneyId { get; set; }
        public Journey Journey { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public JourneyRole Role { get; set; } = JourneyRole.Participant;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
