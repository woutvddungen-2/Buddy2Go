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
        public DateTime? FinishedAt { get; set; } = null;
        public bool IsOwner { get; set; }
        public bool IsParticipant { get; set; }
        public bool CanJoin { get; set; }
    }


}
