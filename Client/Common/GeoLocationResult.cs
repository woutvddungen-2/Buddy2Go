namespace Client.Common
{
    public class GeolocationResult
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double accuracy { get; set; }
        public double? altitude { get; set; }
        public double? altitudeAccuracy { get; set; }
    }
}
