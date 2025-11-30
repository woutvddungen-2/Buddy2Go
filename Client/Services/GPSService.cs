using Microsoft.JSInterop;

namespace Client.Services
{    public class GPSService
    {
        private readonly IJSRuntime js;
        public GPSService(IJSRuntime js)
        {
            this.js = js;
        }

        public async Task<GeolocationResult?> GetGPSAsync()
        {
            try
            {
                return await js.InvokeAsync<GeolocationResult>("geolocationFunctions.getCurrentPosition");
            }
            catch
            {
                return null;
            }
        }
    }

    public class GeolocationResult
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double accuracy { get; set; }
        public double? altitude { get; set; }
        public double? altitudeAccuracy { get; set; }
    }
}
