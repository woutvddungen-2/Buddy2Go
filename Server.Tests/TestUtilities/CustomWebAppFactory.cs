using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Common;
using Server.Infrastructure.Data;
using Server.Tests.Integration.Fakes;


namespace Server.Tests.TestUtilities
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>
    {

        public string DbName { get; } = $"HttpTestDb-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("JwtSettings__Secret", "ThisIsASuperSecretKey12345678901");
            Environment.SetEnvironmentVariable("SSL__Enabled", "false");
            Environment.SetEnvironmentVariable("AllowedOrigins", "http://localhost");

            builder.UseEnvironment("Testing");


            builder.ConfigureServices(services =>
            {
                // ---- Replace DB with InMemory ----
                var dbDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                    .ToList();

                foreach (var d in dbDescriptors)
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DbName);
                });

                // ---- Replace ISmsService with Fake ----
                var smsDescriptors = services
                    .Where(d => d.ServiceType == typeof(ISmsService))
                    .ToList();

                foreach (var d in smsDescriptors)
                    services.Remove(d);

                services.AddSingleton<ISmsService, FakeSmsService>();
            });
        }
    }
}
