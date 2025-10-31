using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models.Dtos;
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

        public async Task<ServiceResult> AddJourneyAsync(JourneyCreateDto dto)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/Journey/AddJourney")
                {
                    Content = JsonContent.Create(dto)
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

        public async Task<ServiceResult> JoinJourneyAsync(int journeyId)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/Journey/JoinJourney/{journeyId}");
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
        

        public async Task<ServiceResult> UpdateJourneyAsync(int journeyId, string? startGps, string? endGps)
        {
            try
            {
                JourneyCreateDto dto = new JourneyCreateDto
                {
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
