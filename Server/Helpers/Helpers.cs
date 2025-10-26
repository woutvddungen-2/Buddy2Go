using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Helpers
{
    public static class DbConnectHelper
    {
        public static void AddDatabase(IServiceCollection services, IConfigurationSection config)
        {
            string server = config["Ip"] ?? "localhost";
            string database = config["DatabaseName"] ?? throw new Exception("Database name missing.");
            string user = config["UserID"] ?? throw new Exception("Database user missing.");
            string password = config["Password"] ?? throw new Exception("Database password missing.");
            string port = config["Port"] ?? "3306";

            if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(database) && !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(port))
            {
                string connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";

                services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }
            else
            {
                throw new Exception("Database configuration is missing or incomplete.");
            }
        }
        public static bool TestDatabaseConnection(IServiceProvider serviceProvider, out Exception? exception)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                bool canConnect = dbContext.Database.CanConnect();
                exception = null;
                return canConnect;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }
    }
}

