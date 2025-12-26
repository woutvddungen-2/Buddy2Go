using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Common;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Server.Tests.TestUtilities;

namespace Server.Tests.Users.Integration
{
    public sealed class UserTestHarness
    {
        public AppDbContext Db { get; }
        public IConfiguration Config { get; }
        public Mock<ISmsService> SmsMock { get; }
        public UserService Service { get; }
        public UserController Controller { get; }

        private UserTestHarness(AppDbContext db, IConfiguration config, Mock<ISmsService> smsMock, UserService service, UserController controller)
        {
            Db = db;
            Config = config;
            SmsMock = smsMock;
            Service = service;
            Controller = controller;
        }

        public static UserTestHarness Create(string dbName)
        {
            var db = InMemoryDbContextFactory.Create(dbName);

            var smsMock = new Mock<ISmsService>(MockBehavior.Strict);
            smsMock.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("OK");

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Secret", "ThisIsASuperSecretKey12345678901" },
                    { "SSL:Enabled", "false" }
                })
                .Build();

            ILogger<UserService> logger = Mock.Of<ILogger<UserService>>();
            var service = new UserService(db, logger, config, smsMock.Object);

            ILogger<UserController> controllerLogger = Mock.Of<ILogger<UserController>>();
            IHostEnvironment env = Mock.Of<IHostEnvironment>();
            var controller = new UserController(service, controllerLogger, env, config);

            //provide HttpContext so Response.Cookies works if your Login writes cookies
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return new UserTestHarness(db, config, smsMock, service, controller);
        }

        public async Task<User> SeedUserAsync(string username = "john", string email = "john@test.com", string phonenumber = "+31612345678", string password = "Password123!")
        {
            User user = new User
            {
                Username = username,
                PasswordHash = UserService.HashPassword(password),
                Email = email,
                Phonenumber = phonenumber,
                CreatedAt = DateTime.UtcNow
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();
            return user;
        }

    }
}
