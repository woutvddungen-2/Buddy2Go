using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models;
using Shared.Models.Dtos;
using System.Net.Http.Json;

namespace Client.Services
{
    public class BuddyService
    {
        private readonly HttpClient httpClient;

        public BuddyService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ServiceResult> SendBuddyRequest(int adresseeId)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/Buddy/Send/{adresseeId}");
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

        public async Task<ServiceResult> RespondToBuddyRequest(int requesterId, RequestStatus status)
        {
            if (status == RequestStatus.Pending)
                return ServiceResult.Fail("Status cannot be changed to pending");
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, $"api/Buddy/Respond")
                {
                    Content = JsonContent.Create(new RequestResponseDto { RequesterId = requesterId, Status = status })
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

        public async Task<ServiceResult<List<BuddyDto>>> GetBuddyList()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"api/Buddy/List");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<BuddyDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<BuddyDto>? data = await response.Content.ReadFromJsonAsync<List<BuddyDto>>();
                return ServiceResult<List<BuddyDto>>.Succes(data ?? new());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<BuddyDto>>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<List<BuddyDto>>> GetPendingRequests()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"api/Buddy/Pending");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<BuddyDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<BuddyDto>? data = await response.Content.ReadFromJsonAsync<List<BuddyDto>>();
                return ServiceResult<List<BuddyDto>>.Succes(data ?? new());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<BuddyDto>>.Fail(ex.Message);
            }
        }
        public async Task<ServiceResult<List<BuddyDto>>> GetSendRequests()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"api/Buddy/GetSend");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<BuddyDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<BuddyDto>? data = await response.Content.ReadFromJsonAsync<List<BuddyDto>>();
                return ServiceResult<List<BuddyDto>>.Succes(data ?? new());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<BuddyDto>>.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult> RemoveBuddy(int buddyId, bool block = false)
        {
            try
            {
                HttpRequestMessage request;
                if (block)
                    request = new HttpRequestMessage(HttpMethod.Patch, $"api/Buddy/Block/{buddyId}");                
                else
                    request = new HttpRequestMessage(HttpMethod.Delete, $"api/Buddy/Delete/{buddyId}");

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
    }
}
