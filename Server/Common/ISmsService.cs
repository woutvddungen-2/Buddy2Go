namespace Server.Common
{
    public interface ISmsService
    {
        Task<ServiceResult> SendSmsAsync(string toNumber, string body);
        
    }

}

