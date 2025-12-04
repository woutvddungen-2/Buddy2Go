using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Server.Services
{
    public class SmsService
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

        public async Task<string> SendSmsAsync(string toNumber, string body)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "To", toNumber },
                { "From", from },
                { "Body", body }
            });

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json";

            HttpResponseMessage resp = await client.PostAsync(url, form);
            resp.EnsureSuccessStatusCode();

            string json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("sid").GetString()!;
        }
    }
}
