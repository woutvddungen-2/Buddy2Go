using Client.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Shared.Models.Dtos;
using System.Net.Http.Json;

namespace Client.Services
{
    public class ChatService
    {
        private readonly HttpClient httpClient;

        public ChatService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Get all chat messages for a Journey.
        /// </summary>
        public async Task<ServiceResult<List<JourneyMessageDto>>> GetMessagesAsync(int journeyId)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Chat/journey/{journeyId}"
                );
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return ServiceResult<List<JourneyMessageDto>>.Fail(await response.Content.ReadAsStringAsync());

                List<JourneyMessageDto>? dto = await response.Content.ReadFromJsonAsync<List<JourneyMessageDto>>();
                if (dto == null)
                    return ServiceResult<List<JourneyMessageDto>>.Succes(new List<JourneyMessageDto>());
                return ServiceResult<List<JourneyMessageDto>>.Succes(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<JourneyMessageDto>>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Send a message to a journey group chat.
        /// </summary>
        public async Task<ServiceResult<JourneyMessageDto>> SendMessageAsync(int journeyId, string content)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/Chat/journey/{journeyId}")
                {
                    Content = JsonContent.Create(new JourneyMessageCreateDto { Content = content })
                };

                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return ServiceResult<JourneyMessageDto>.Fail(await response.Content.ReadAsStringAsync());

                JourneyMessageDto? dto = await response.Content.ReadFromJsonAsync<JourneyMessageDto>();
                if (dto == null)
                    return ServiceResult<JourneyMessageDto>.Fail("Invalid chat message data");

                return ServiceResult<JourneyMessageDto>.Succes(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<JourneyMessageDto>.Fail(ex.Message);
            }
        }
    }
}
