using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Common;
using Server.Features.Users;
using Shared.Models.Dtos.Users;
using System.Security.Claims;

namespace Server.Tests.Controllers
{
    public class UserControllerTests
    {
        // ------------------ StartRegister ------------------

        [Fact]
        public async Task StartRegister_ShouldReturnBadRequest_WhenNull()
        {
            var serviceMock = new Mock<IUserService>();
            var controller = CreateController(serviceMock);

            IActionResult result = await controller.StartRegister(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task StartRegister_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock
                .Setup(s => s.StartRegistrationAsync("john", "Pass1234!", "a@b.com", "+31612345678"))
                .ReturnsAsync(ServiceResult.Succes("ok"));

            var controller = CreateController(serviceMock);

            var dto = new RegisterDto
            {
                Username = "john",
                Password = "Pass1234!",
                Email = "a@b.com",
                PhoneNumber = "0612345678"
            };

            IActionResult result = await controller.StartRegister(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ VerifyRegister ------------------

        [Fact]
        public async Task VerifyRegister_ShouldReturnBadRequest_WhenNull()
        {
            var controller = CreateController(new Mock<IUserService>());
            var result = await controller.VerifyRegister(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task VerifyRegister_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock
                .Setup(s => s.CompleteRegistrationAsync("+31611111111", "123456"))
                .ReturnsAsync(ServiceResult.Succes("done"));

            var controller = CreateController(serviceMock);
            var dto = new VerifyUserDto { PhoneNumber = "0611111111", Code = "123456" };

            IActionResult result = await controller.VerifyRegister(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ Login ------------------

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenNull()
        {
            var controller = CreateController(new Mock<IUserService>());
            var result = await controller.Login(null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock.Setup(s => s.Login("john", "pass"))
                       .ReturnsAsync(ServiceResult<string>.Succes("jwt_token"));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var dto = new LoginDto { Identifier = "john", Password = "pass" };

            IActionResult result = await controller.Login(dto);
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ GetUserInfo ------------------

        [Fact]
        public async Task GetUserInfo_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock
                .Setup(s => s.GetUserInfo(1))
                .ReturnsAsync(ServiceResult<UserDto>.Succes(new UserDto { Id = 1, Username = "A" }));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            IActionResult result = await controller.GetUserInfo();
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ ReturnJWT ------------------

        [Fact]
        public void ReturnJWT_ShouldReturnOk_WhenTokenValid()
        {
            var controller = CreateController(new Mock<IUserService>());
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1, "Alice");

            IActionResult result = controller.Verify();
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ Logout ------------------

        [Fact]
        public void Logout_ShouldReturnOk()
        {
            var controller = CreateController(new Mock<IUserService>());
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            IActionResult result = controller.Logout();
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ FindByPhone ------------------

        [Fact]
        public async Task FindByPhone_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock.Setup(s => s.FindUserbyPhone("+31611111111", 1))
                       .ReturnsAsync(ServiceResult<UserDto>.Succes(new UserDto { Id = 1 }));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            IActionResult result = await controller.FindbyPhoneNumber("0611111111");
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ UpdateEmail ------------------

        [Fact]
        public async Task UpdateEmail_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock.Setup(s => s.UpdateEmailAsync(1, "a@b.com"))
                       .ReturnsAsync(ServiceResult.Succes("ok"));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            IActionResult result = await controller.UpdateEmail("a@b.com");
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ UpdatePhoneNumber ------------------

        [Fact]
        public async Task UpdatePhone_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock.Setup(s => s.UpdatePhoneNumberAsync(1, "+31611111111"))
                       .ReturnsAsync(ServiceResult.Succes("ok"));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            IActionResult result = await controller.UpdatePhoneNumber("0611111111");
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ UpdatePassword ------------------

        [Fact]
        public async Task UpdatePassword_ShouldReturnBadRequest_WhenNull()
        {
            var controller = CreateController(new Mock<IUserService>());
            var result = await controller.UpdatePassword(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePassword_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock.Setup(s => s.UpdatePasswordAsync(1, "old", "new"))
                       .ReturnsAsync(ServiceResult.Succes("changed"));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            var dto = new ChangePasswordDto { OldPassword = "old", NewPassword = "new" };
            IActionResult result = await controller.UpdatePassword(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ DeleteUser ------------------

        [Fact]
        public async Task DeleteUser_ShouldReturnOk_WhenSuccess()
        {
            var serviceMock = new Mock<IUserService>();
            serviceMock.Setup(s => s.DeleteUserAsync(1))
                       .ReturnsAsync(ServiceResult.Succes("done"));

            var controller = CreateController(serviceMock);
            controller.ControllerContext.HttpContext = MockHttpContextWithUserId(1);

            IActionResult result = await controller.DeleteUser();
            Assert.IsType<OkObjectResult>(result);
        }

        // ------------------ Helpers ----------------------

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

        private IConfiguration BuildConfig(bool ssl)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "SSL:Enabled", ssl ? "true" : "false" }
                })
                .Build();
        }

        private static HttpContext MockHttpContextWithUserId(int id, string username = "User")
        {
            var context = new DefaultHttpContext();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username)
            }, "jwt");

            context.User = new ClaimsPrincipal(identity);

            return context;
        }
    }
}
