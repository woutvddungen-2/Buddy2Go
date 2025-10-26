namespace Server.Models
{
    public class Journey
    {
        public int Id { get; set; }
        public int OwnedBy { get; set; }
        public string StartGPS { get; set; } = string.Empty;
        public string EndGPS { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        // Navigation
        public User? Owner { get; set; }
        public ICollection<JourneyParticipants> Participants { get; set; } = null!;
        public ICollection<JourneyMessages> Messages { get; set; } = null!;
    }
}
