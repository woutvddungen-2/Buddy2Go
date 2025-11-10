namespace Shared.Models.Dtos
{
    public class JourneyDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string StartGPS { get; set; } = string.Empty;
        public string EndGPS { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime StartAt { get; set; } 
        public DateTime? FinishedAt { get; set; } = null;
        public List<JourneyParticipantDto> Participants { get; set; } = new List<JourneyParticipantDto>();
    }


}
