using Server.Features.Chats;

namespace Server.Features.Journeys
{
    public class Journey
    {
        public int Id { get; set; }
        public int StartId { get; set; }
        public int EndId { get; set; }

        public Place Start { get; set; } = null!;
        public Place End { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime StartAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; } = null;

        // Navigation
        public ICollection<JourneyParticipant> Participants { get; set; } = null!;
        public ICollection<JourneyMessage> Messages { get; set; } = null!;
        public ICollection<Rating> Ratings { get; set; } = null!;
    }
}
