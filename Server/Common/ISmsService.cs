namespace Server.Common
{
    public interface ISmsService
    {
        Task<string> SendSmsAsync(string toNumber, string body);
        
    }

}

