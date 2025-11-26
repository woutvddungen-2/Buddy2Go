using Server.Features.Users;
using Shared.Models.enums;

namespace Server.Features.DangerousPlaces
{
    public class DangerousPlace
    {
        public int Id { get; set; }
        public int ReportedById { get; set; }
        public User ReportedBy { get; set; } = null!;

        public DangerousPlaceType PlaceType { get; set; } = DangerousPlaceType.Other;
        public string? Description { get; set; }
        public string GPS { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
    }

}
