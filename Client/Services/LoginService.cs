using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models.Dtos;
using System.Net.Http.Json;

namespace Client.Services
{
    public class LoginService
    {
        private readonly HttpClient httpClient;

        public LoginService(HttpClient httpClient)
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

        public async Task<ServiceResult> IsLoggedInAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/User/verify");
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/User/verify");
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
    }
}
