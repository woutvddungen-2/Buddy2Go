using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models.Dtos.DangerousPlaces;
using Shared.Models.enums;
using System.Net.Http.Json;

namespace Client.Services
{
    public class DangerousPlaceService
    {
        private readonly HttpClient httpClient;
        public DangerousPlaceService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Get all chat messages for a Journey.
        /// </summary>
        public async Task<ServiceResult<List<DangerousPlaceDto>>> GetMyReportsAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"api/DangerousPlace/GetMyReports");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<DangerousPlaceDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<DangerousPlaceDto>? dto = await response.Content.ReadFromJsonAsync<List<DangerousPlaceDto>>();
                if (dto == null)
                    return ServiceResult<List<DangerousPlaceDto>>.Succes(new List<DangerousPlaceDto>());
                return ServiceResult<List<DangerousPlaceDto>>.Succes(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<DangerousPlaceDto>>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Send a message to a journey group chat.
        /// </summary>
        public async Task<ServiceResult> CreateReportAsync(DangerousPlaceType placeType, string? description, string gps)
        {
            if (String.IsNullOrWhiteSpace(description))
                description = null;
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/DangerousPlace/CreateReport")
                {
                    Content = JsonContent.Create(new DangerousPlaceCreateDto { PlaceType = placeType, Description = description, GPS = gps})
                };
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
                return ServiceResult.Succes();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Send a message to a journey group chat.
        /// </summary>
        public async Task<ServiceResult> UpdateReportAsync(int id, DangerousPlaceType placeType, string? description, string gps)
        {
            if (String.IsNullOrWhiteSpace(description))
                description = null;
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/DangerousPlace/UpdateReport")
                {
                    Content = JsonContent.Create(new DangerousPlaceCreateDto { Id = id, PlaceType = placeType, Description = description, GPS = gps })
                };
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
                return ServiceResult.Succes();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

    }
}
