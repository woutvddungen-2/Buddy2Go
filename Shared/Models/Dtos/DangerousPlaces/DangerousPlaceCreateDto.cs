using Shared.Models.enums;

namespace Shared.Models.Dtos.DangerousPlaces
{
    public class DangerousPlaceCreateDto
    {
        public int Id { get; set; }
        public DangerousPlaceType PlaceType { get; set; } = DangerousPlaceType.Other;
        public string? Description { get; set; }
        public string GPS { get; set; } = string.Empty;
    }
}
