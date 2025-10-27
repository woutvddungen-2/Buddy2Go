namespace Shared.Models.Dtos
{
    public class BuddyDto
    {
        public int RequesterId { get; set; }
        public int AddresseeId { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
