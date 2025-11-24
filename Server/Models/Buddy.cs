using Shared.Models.enums;
namespace Server.Models
{
    public class Buddy
    {
        public int RequesterId { get; set; }
        public int AddresseeId { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public User Requester { get; set; } = null!;
        public User Addressee { get; set; } = null!;
    }
}
