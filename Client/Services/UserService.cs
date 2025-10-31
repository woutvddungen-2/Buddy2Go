using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models.Dtos;
using System.Net.Http.Json;

namespace Client.Services
{
    public class UserService
    {
        private readonly HttpClient httpClient;

        public UserService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ServiceResult> LoginAsync(string username, string password)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/User/login")
            {
                Content = JsonContent.Create(new LoginDto { Username = username, Password = password })
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage? response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return ServiceResult.Succes();

            return ServiceResult.Fail(await response.Content.ReadAsStringAsync());

        }

        public async Task<ServiceResult> LogoutAsync()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/User/logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage? response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return ServiceResult.Succes();
            return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
        }

        public async Task<ServiceResult<int>> GetUserId()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/User/Verify");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return ServiceResult<int>.Fail("Failed to parse user data.");
                }
                UserDto? data = await response.Content.ReadFromJsonAsync<UserDto>();
                if (data == null)
                {
                    return ServiceResult<int>.Fail("Failed to parse user data.");
                }

                return ServiceResult<int>.Succes(data.Id);
            }
            catch
            {
                return ServiceResult<int>.Fail("Failed to parse user data.");
            }
        }

        public async Task<ServiceResult> IsLoggedInAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/User/Verify");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage? response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return ServiceResult.Succes();
                return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        public async Task<ServiceResult<UserDto?>> GetCurrentUserAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/User/GetUserInfo");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                HttpResponseMessage? response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<UserDto?>.Fail(await response.Content.ReadAsStringAsync());
                UserDto? dto =  await response.Content.ReadFromJsonAsync<UserDto>();
                if (dto == null)
                    return ServiceResult<UserDto?>.Fail("Failed to parse user data.");
                return ServiceResult<UserDto?>.Succes(dto);
            }
            catch
            {
                return ServiceResult<UserDto?>.Fail("Failed to parse user data.");
            }
        }

        public async Task<ServiceResult> RegisterAsync(string username, string password, string email, string phonenumber)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/User/register")
            {
                Content = JsonContent.Create(new RegisterDto { Username = username, Password = password, Email = email, PhoneNumber = phonenumber })
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage? response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return ServiceResult.Succes();
            return ServiceResult.Fail(await response.Content.ReadAsStringAsync());
        }

        public async Task<ServiceResult<List<BuddyDto>>> FindUserbyPhone(string Phonenumber)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/User/FindbyPhonenumber");
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
        //public async Task<ServiceResult<List<BuddyDto>>> FindUserbyEmail(string Email)
        //{
        //    try
        //    {
        //        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"api/Buddy/Pending");
        //        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        //        HttpResponseMessage? response = await httpClient.SendAsync(request);

        //        if (!response.IsSuccessStatusCode)
        //            return ServiceResult<List<BuddyDto>>.Fail(await response.Content.ReadAsStringAsync());

        //        List<BuddyDto>? data = await response.Content.ReadFromJsonAsync<List<BuddyDto>>();
        //        return ServiceResult<List<BuddyDto>>.Succes(data ?? new());
        //    }
        //    catch (Exception ex)
        //    {
        //        return ServiceResult<List<BuddyDto>>.Fail(ex.Message);
        //    }
        //}
    }
}
