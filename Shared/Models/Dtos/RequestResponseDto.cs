using Shared.Models.enums;

namespace Shared.Models.Dtos
{
    public class RequestResponseDto
    {
        public int RequesterId { get; set; }
        public RequestStatus Status { get; set; }
    }
}
