using Server.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Server.Services
{
    public class SmsService: ISmsService
    {
        private readonly HttpClient client;
        private readonly string sid;
        private readonly string token;
        private readonly string from;

        public SmsService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient();

            sid = config["Twilio:AccountSid"]
                     ?? throw new Exception("Twilio AccountSid missing");
            token = config["Twilio:AuthToken"]
                     ?? throw new Exception("Twilio AuthToken missing");
            from = config["Twilio:FromNumber"]
                     ?? throw new Exception("Twilio FromNumber missing");

            // Attach basic auth header
            var bytes = Encoding.ASCII.GetBytes($"{sid}:{token}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
        }

        public async Task<ServiceResult> SendSmsAsync(string toNumber, string body)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "To", toNumber },
                { "From", from },
                { "Body", body }
            });

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json";
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(url, form);
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult.Fail(ServiceResultStatus.ExternalServiceError, $"Failed to reach SMS provider: {ex.Message}");
            }
            if (!response.IsSuccessStatusCode)
                return ServiceResult.Fail(ServiceResultStatus.ExternalServiceError,$"SMS provider returned {(int)response.StatusCode} ({response.StatusCode})");

            string json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("sid", out var sidElement))
                    return ServiceResult.Fail(ServiceResultStatus.UnexpectedError,"SMS provider response did not contain a message SID");
                
                string? messageSid = sidElement.GetString();
                if (string.IsNullOrWhiteSpace(messageSid))
                    return ServiceResult.Fail(ServiceResultStatus.UnexpectedError, "SMS provider returned an empty message SID");

                return ServiceResult.Succes(messageSid);
            }
            catch (JsonException)
            {
                return ServiceResult.Fail(ServiceResultStatus.UnexpectedError, "Failed to parse SMS provider response");
            }
        }
    }
}
