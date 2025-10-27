namespace Shared.Models.Dtos
{
    public class JourneyDto
    {
        public int Id { get; set; }
        public int OwnedBy { get; set; }
        public string StartGPS { get; set; } = string.Empty;
        public string EndGPS { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime FinishedAt { get; set; }
    }
}
