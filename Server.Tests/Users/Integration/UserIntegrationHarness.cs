using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Common;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Server.Tests.TestUtilities;
using System.Security.Claims;

namespace Server.Tests.Users.Integration
{
    public class UserIntegrationHarness
    {
        public AppDbContext Db { get; }
        public IConfiguration Config { get; }
        public Mock<ISmsService> SmsMock { get; }
        public UserService Service { get; }
        public UserController Controller { get; }

        public UserIntegrationHarness(string dbName)
        {
            Db = InMemoryDbContextFactory.Create(dbName);

            SmsMock = new Mock<ISmsService>(MockBehavior.Strict);
            SmsMock.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("OK");

            Config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Secret", "ThisIsASuperSecretKey12345678901" },
                    { "SSL:Enabled", "false" }
                })
                .Build();

            ILogger<UserService> logger = Mock.Of<ILogger<UserService>>();
            Service = new UserService(Db, logger, Config, SmsMock.Object);

            ILogger<UserController> controllerLogger = Mock.Of<ILogger<UserController>>();
            IHostEnvironment env = Mock.Of<IHostEnvironment>();
            Controller = new UserController(Service, controllerLogger, env, Config);

            Controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        public async Task<User> CreateUserAsync(string username, string password, string phonenumber)
        {
            User user = new User
            {
                Username = username,
                PasswordHash = UserService.HashPassword(password),
                Email = $"{username}@test.nl",
                Phonenumber = phonenumber,
                CreatedAt = DateTime.UtcNow
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();
            return user;
        }

        public void SetAuthenticatedUser(int userId, string username)
        {
            Claim[] claims =
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            ];

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            Controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}

