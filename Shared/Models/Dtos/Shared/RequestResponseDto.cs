using Shared.Models.enums;

namespace Shared.Models.Dtos.Shared
{
    public class RequestResponseDto
    {
        public int RequesterId { get; set; }
        public RequestStatus Status { get; set; }
    }
}
