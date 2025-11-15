using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models;
using Shared.Models.Dtos;
using System.Linq.Expressions;
using System.Net.Http.Json;

namespace Client.Services
{
    public class JourneyService
    {
        private readonly HttpClient httpClient;

        public JourneyService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ServiceResult<List<JourneyDto>>> GetMyJourneysAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/Journey/GetMyJourneys");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<JourneyDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<JourneyDto>? data = await response.Content.ReadFromJsonAsync<List<JourneyDto>>();
                return ServiceResult<List<JourneyDto>>.Succes(data ?? new());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<JourneyDto>>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<List<JourneyDto>>> GetBuddyJourneysAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/Journey/GetBuddyJourneys");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<JourneyDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<JourneyDto>? data = await response.Content.ReadFromJsonAsync<List<JourneyDto>>();
                return ServiceResult<List<JourneyDto>>.Succes(data ?? new());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<JourneyDto>>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult> AddJourneyAsync(int startPlaceId, int endPlaceId, DateTime startAtUtc)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/Journey/AddJourney")
                {
                    Content = JsonContent.Create(new JourneyCreateDto { StartPlaceId = startPlaceId, EndPlaceId = endPlaceId, StartAt = startAtUtc })
                };
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

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
        /// Sends a request to join a journey.
        /// </summary>
        public async Task<ServiceResult> SendJoinRequestAsync(int journeyId)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/Journey/SendJoinRequest/{journeyId}");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
                return ServiceResult.Succes(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }

        }

        /// <summary>
        /// Responds to a pending join request (Accept or Reject).
        /// </summary>
        public async Task<ServiceResult> RespondToJoinRequestAsync(int journeyId, int requesterId, RequestStatus status)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"api/Journey/RespondToJoinRequest/{journeyId}")
                {
                    Content = JsonContent.Create(new RequestResponseDto { RequesterId = requesterId, Status = status})
                };
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
                return ServiceResult.Succes(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex) 
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult> FinishJourney (int journeyId)
        {
            try
            {
                HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Patch, $"api/Journey/FinishJourney/{journeyId}");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
                return ServiceResult.Succes();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }
        

        public async Task<ServiceResult> UpdateJourneyAsync(int journeyId, int startPlaceId, int endPlaceId, DateTime startAtUtc)
        {
            try
            {
                JourneyCreateDto dto = new JourneyCreateDto
                {
                    StartAt = startAtUtc,
                    StartPlaceId = startPlaceId,
                    EndPlaceId = endPlaceId
                };
                HttpRequestMessage request = new(HttpMethod.Patch, $"api/Journey/UpdateGPS/{journeyId}")
                {
                    Content = JsonContent.Create(dto)
                };
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return ServiceResult.Succes();

                return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult> LeaveJourneyAsync(int journeyId)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Delete, $"api/Journey/LeaveJourney/{journeyId}");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return ServiceResult.Succes();

                return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<List<PlaceDto>>> GetPlacesAsync()
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, $"api/Journey/GetPlaces");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<PlaceDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<PlaceDto>? data = await response.Content.ReadFromJsonAsync<List<PlaceDto>>();
                return ServiceResult<List<PlaceDto>>.Succes(data ?? new());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PlaceDto>>.Fail(ex.Message);
            }
        }
    }
}
