using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Common;
using Server.Features.Users;
using Server.Tests.TestUtilities;
using Shared.Models.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests.Users
{
    public class UserIntegrationTests
    {

        [Fact]
        public async Task FullRegistrationFlow_ShouldCreateVerifiedUser()
        {
            // ───────────────────────────────────────────────────────────────
            // Arrange: Setup InMemory DB + Mock SMS service
            // ───────────────────────────────────────────────────────────────
            var db = InMemoryDbContextFactory.Create("RegisterFlowTest");

            var smsMock = new Mock<ISmsService>();
            smsMock.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync("OK");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Secret", "ThisIsASuperSecretKey12345678901" },
                    { "SSL:Enabled", "false" }
                })
                .Build();

            var logger = Mock.Of<ILogger<UserService>>();
            var service = new UserService(db, logger, config, smsMock.Object);

            var controllerLogger = Mock.Of<ILogger<UserController>>();
            var env = Mock.Of<IHostEnvironment>();
            var controller = new UserController(service, controllerLogger, env, config);

            // ───────────────────────────────────────────────────────────────
            // Step 1: Call StartRegister
            // ───────────────────────────────────────────────────────────────
            RegisterDto dto = new()
            {
                Username = "john",
                Password = "Password123!",
                Email = "john@test.com",
                PhoneNumber = "0612345678"
            };

            IActionResult startResult = await controller.StartRegister(dto);

            Assert.IsType<OkObjectResult>(startResult);

            // Verify SMS was triggered
            smsMock.Verify(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // ───────────────────────────────────────────────────────────────
            // Step 2: Retrieve verification code from DB
            // ───────────────────────────────────────────────────────────────
            var verification = db.UserVerifications.FirstOrDefault();
            Assert.NotNull(verification);

            string code = verification!.Code;
            string normalizedPhone = verification!.PhoneNumber;

            // ───────────────────────────────────────────────────────────────
            // Step 3: Call VerifyRegister
            // ───────────────────────────────────────────────────────────────
            var verifyDto = new VerifyUserDto
            {
                PhoneNumber = normalizedPhone,
                Code = code
            };

            IActionResult verifyResult = await controller.VerifyRegister(verifyDto);

            Assert.IsType<OkObjectResult>(verifyResult);

            // ───────────────────────────────────────────────────────────────
            // Step 4: Verify user was created in DB
            // ───────────────────────────────────────────────────────────────
            var user = db.Users.FirstOrDefault(u => u.Username == "john");

            Assert.NotNull(user);
            Assert.Equal("john@test.com", user!.Email);
            Assert.Equal(normalizedPhone, user!.Phonenumber);
        }
    }
}
