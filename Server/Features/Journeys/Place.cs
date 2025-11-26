namespace Server.Features.Journeys
{
    public class Place
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string? District { get; set; }
        public string CentreGPS { get; set; } = string.Empty;
    }
}
