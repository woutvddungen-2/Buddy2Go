using Microsoft.AspNetCore.Mvc;
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
        private UserController CreateController(Mock<IUserService> svcMock)
        {
            var logger = Mock.Of<ILogger<UserController>>();
            var env = Mock.Of<IHostEnvironment>();

            return new UserController(svcMock.Object, logger, env);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenNull()
        {
            var serviceMock = new Mock<IUserService>();
            var controller = CreateController(serviceMock);

            var result = await controller.Register(null!);

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
            var controller = CreateController(serviceMock);

            var dto = new RegisterDto
            {
                Username = "john",
                Password = "Pass1234!",
                Email = "a@b.com",
                PhoneNumber = "0612345678"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
