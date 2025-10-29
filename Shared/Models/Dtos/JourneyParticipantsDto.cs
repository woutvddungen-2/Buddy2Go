namespace Shared.Models.Dtos
{
    public class JourneyParticipantDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public JourneyRole Role { get; set; } = JourneyRole.Participant;
        public DateTime JoinedAt { get; set; }
    }
}
