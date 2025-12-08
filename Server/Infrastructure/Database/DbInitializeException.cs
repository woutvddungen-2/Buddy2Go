namespace Server.Infrastructure.Database
{
    public class DbInitializeException : Exception
    {
        public DbInitializeException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
