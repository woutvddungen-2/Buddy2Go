namespace Client.Common
{
    // Base class for all service results
    public class ServiceResultBase
    {
        public bool IsSuccess { get; protected set; }
        public string? Message { get; protected set; }

        protected ServiceResultBase(bool isSuccess, string? message = null)
        {
            IsSuccess = isSuccess;
            Message = message;
        }
    }

    // Non-generic result (for void-like operations)
    public class ServiceResult : ServiceResultBase
    {
        private ServiceResult(bool isSuccess, string? message = null)
            : base(isSuccess, message) { }

        public static ServiceResult Succes(string? message = null) => new(true, message);
        public static ServiceResult Fail(string message) => new(false, message);
    }

    // Generic result (for operations returning data)
    public class ServiceResult<T> : ServiceResultBase
    {
        public T? Data { get; private set; }

        private ServiceResult(bool isSuccess, T? data = default, string? message = null)
            : base(isSuccess, message)
        {
            Data = data;
        }

        public static ServiceResult<T> Succes(T data, string? message = null) => new(true, data, message);
        public static ServiceResult<T> Fail(string? message = null) => new(false, default, message);
    }
}
