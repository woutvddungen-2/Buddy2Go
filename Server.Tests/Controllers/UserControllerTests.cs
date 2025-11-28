using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Common;
using Server.Features.Users;
using Shared.Models.Dtos;

namespace Server.Tests.Controllers
{
    public class UserControllerTests
    {
        private UserController CreateController(Mock<IUserService> svcMock, bool ssl = false)
        {
            var logger = Mock.Of<ILogger<UserController>>();
            var env = Mock.Of<IHostEnvironment>();

            return new UserController(
                svcMock.Object,
                logger,
                env,
                BuildConfig(ssl)
            );
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenNull()
        {
            var serviceMock = new Mock<IUserService>();
            var controller = CreateController(serviceMock);

            IActionResult result = await controller.Register(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var serviceMock = new Mock<IUserService>();
            serviceMock
                .Setup(s => s.Register("john", "Pass1234!", "a@b.com", "0612345678"))
                .ReturnsAsync(ServiceResult.Succes("ok"));
            UserController controller = CreateController(serviceMock);

            RegisterDto dto = new RegisterDto
            {
                Username = "john",
                Password = "Pass1234!",
                Email = "a@b.com",
                PhoneNumber = "0612345678"
            };

            // Act
            IActionResult result = await controller.Register(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        private IConfiguration BuildConfig(bool ssl)
        {
            Dictionary<string, string?> configValues = new()
            {
                { "SSL:Enabled", ssl ? "true" : "false" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        }
            
    }
}
