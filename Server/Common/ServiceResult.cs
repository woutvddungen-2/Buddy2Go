namespace Server.Common
{
    // Base class for all service results
    public class ServiceResultBase
    {
        public ServiceResultStatus Status { get; protected set; }
        public string? Message { get; protected set; }

        protected ServiceResultBase(ServiceResultStatus status, string? message = null)
        {
            Status = status;
            Message = message;
        }

        public bool IsSuccess => Status == ServiceResultStatus.Success;
    }

    // Non-generic result (for void-like operations)
    public class ServiceResult : ServiceResultBase
    {
        private ServiceResult(ServiceResultStatus status, string? message = null) : base(status, message) { }

        public static ServiceResult Succes(string? message = null) => new(ServiceResultStatus.Success, message);
        public static ServiceResult Fail(ServiceResultStatus status, string message) => new(status, message);
    }

    // Generic result (for operations returning data)
    public class ServiceResult<T> : ServiceResultBase
    {
        public T? Data { get; private set; }

        private ServiceResult(ServiceResultStatus status, T? data = default, string? message = null)
            : base(status, message)
        {
            Data = data;
        }

        public static ServiceResult<T> Succes(T data, string? message = null) => new(ServiceResultStatus.Success, data, message);
        public static ServiceResult<T> Fail(ServiceResultStatus status, string? message = null) => new(status, default, message);
    }
}
