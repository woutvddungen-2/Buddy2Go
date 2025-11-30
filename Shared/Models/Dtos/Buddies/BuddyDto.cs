using Shared.Models.Dtos.Users;
using Shared.Models.enums;

namespace Shared.Models.Dtos.Buddies
{
    public class BuddyDto
    {
        public UserDto Requester { get; set; } = new UserDto();
        public UserDto Addressee { get; set; } = new UserDto();
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
