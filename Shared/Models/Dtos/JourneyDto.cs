namespace Shared.Models.Dtos
{
    public class JourneyDto
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;

        public required PlaceDto Start { get; set; }
        public required PlaceDto End { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime StartAt { get; set; } 
        public DateTime? FinishedAt { get; set; } = null;
        public List<JourneyParticipantDto> Participants { get; set; } = new List<JourneyParticipantDto>();
    }


}
