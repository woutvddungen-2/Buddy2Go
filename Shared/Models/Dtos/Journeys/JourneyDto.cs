namespace Shared.Models.Dtos.Journeys
{
    public class JourneyDto
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;

        public PlaceDto Start { get; set; } = null!;
        public PlaceDto End { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime StartAt { get; set; } 
        public DateTime? FinishedAt { get; set; } = null;
        public List<JourneyParticipantDto> Participants { get; set; } = new List<JourneyParticipantDto>();
    }


}
