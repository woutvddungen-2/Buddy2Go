namespace Server.Common
{    
    public enum ServiceResultStatus
    {
        Success,
        ResourceNotFound,
        UserNotFound,
        Unauthorized,
        ValidationError,
        InvalidOperation,
        Blocked,
        Error
    }

}
