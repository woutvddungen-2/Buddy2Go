namespace Shared.Models.Dtos
{
    public class PlaceDto
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string? District { get; set; }
        public string CentreGPS { get; set; } = string.Empty;
    }
}
