using Microsoft.JSInterop;

namespace Client.Common
{    public class GPSService
    {
        private readonly IJSRuntime js;
        public GPSService(IJSRuntime js)
        {
            this.js = js;
        }

        public async Task<ServiceResult<GeolocationResult>> GetGPSAsync()
        {
            try
            {
                GeolocationResult data = await js.InvokeAsync<GeolocationResult>("geolocationFunctions.getCurrentPosition");
                return ServiceResult<GeolocationResult>.Succes(data);
            }
            catch (Exception ex)
            {
                return ServiceResult<GeolocationResult>.Fail($"Geolocation function error: {ex.Message}");
            }
        }
    }
}
