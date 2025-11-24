using Shared.Models.enums;

namespace Shared.Models.Dtos
{
    public class DangerousPlaceDto
    {
        public int Id { get; set; }
        public int ReportedById { get; set; }
        public DangerousPlaceType PlaceType { get; set; } = DangerousPlaceType.Other;
        public string? Description { get; set; }
        public string GPS { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
    }
}
