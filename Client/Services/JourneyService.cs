using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models;
using Shared.Models.Dtos;
using System.Linq.Expressions;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

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

        public async Task<ServiceResult> AddJourneyAsync(string? startGPS, string? endGPS, DateTime startAt)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/Journey/AddJourney")
                {
                    Content = JsonContent.Create(new JourneyCreateDto { StartGPS = startGPS, EndGPS = endGPS, StartAt = startAt })
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/Journey/RespondToJoinRequest/{journeyId}")
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
        

        public async Task<ServiceResult> UpdateJourneyAsync(int journeyId, string? startGps, string? endGps, DateTime startAt)
        {
            try
            {
                JourneyCreateDto dto = new JourneyCreateDto
                {
                    StartAt = startAt,
                    StartGPS = startGps,
                    EndGPS = endGps
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
    }
}
