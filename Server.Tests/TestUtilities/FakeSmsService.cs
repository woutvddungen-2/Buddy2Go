using Server.Common;
using Server.Features.Users;

namespace Server.Tests.Integration.Fakes
{
    public sealed class FakeSmsService : ISmsService
    {
        public List<(string Phone, string Message)> Sent { get; } = new();

        public Task<string> SendSmsAsync(string phoneNumber, string message)
        {
            Sent.Add((phoneNumber, message));
            return Task.FromResult("OK");
        }
    }
}
