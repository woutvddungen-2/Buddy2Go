using Microsoft.AspNetCore.Mvc;
using Moq;
using Server.Features.Users;
using Shared.Models.Dtos.Users;

namespace Server.Tests.Users.Integration
{
    public class UserIntegrationTests
    {

        // ---------------- Registration flow test ----------------
        [Fact]
        public async Task FullRegistrationTest()
        {
            // Arrange: Setup InMemory DB + Mock SMS service
            UserTestHarness harness = new UserTestHarness(nameof(FullRegistrationTest));

            // Step 1: Call StartRegister
            RegisterDto dto = new()
            {
                Username = "john",
                Password = "Password123!",
                Email = "john@test.com",
                PhoneNumber = "0612345678"
            };

            IActionResult startResult = await harness.Controller.StartRegister(dto);

            Assert.IsType<OkObjectResult>(startResult);
            harness.SmsMock.Verify(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // Step 2: Retrieve verification code from DB
            UserVerification? verification = harness.Db.UserVerifications.FirstOrDefault();
            Assert.NotNull(verification);

            string code = verification!.Code;
            string normalizedPhone = verification!.PhoneNumber;

            // Step 3: Call VerifyRegister
            VerifyUserDto verifyDto = new VerifyUserDto
            {
                PhoneNumber = normalizedPhone,
                Code = code
            };

            IActionResult verifyResult = await harness.Controller.VerifyRegister(verifyDto);

            Assert.IsType<OkObjectResult>(verifyResult);

            // Step 4: Verify user was created in DB
            User? user = harness.Db.Users.FirstOrDefault(u => u.Username == "john");

            Assert.NotNull(user);
            Assert.Equal(normalizedPhone, user!.Phonenumber);
            Assert.Equal(dto.Email, user!.Email);
            Assert.Equal(dto.Username, user!.Username);
        }

        // ---------------- Login flow test ----------------
        [Fact]
        public async Task FullLoginTest()
        {
            // Setup: Seed a user in the InMemory DB
            UserTestHarness harness = new UserTestHarness(nameof(FullLoginTest));
            User Seeduser = await harness.SeedUserAsync("john", "Password123!", "+3161000000");

            User? user = harness.Db.Users.FirstOrDefault(u => u.Username == "john");
            Assert.Equal(Seeduser, user);

            // Login
            LoginDto loginDto = new()
            {
                Identifier = "john",
                Password = "Password123!"
            };

            IActionResult loginResult = await harness.Controller.Login(loginDto);

            // Assert: expected OK 
            var ok = Assert.IsType<OkObjectResult>(loginResult);

            var tokenProp = ok.Value?.GetType().GetProperty("token");
            Assert.NotNull(tokenProp);

            var token = tokenProp!.GetValue(ok.Value) as string;
            Assert.False(string.IsNullOrWhiteSpace(token));
        }


        // ---------------- null body tests ----------------
        [Fact]
        public async Task StartRegister_ShouldReturnBadRequest_WhenBodyNull()
        {
            UserTestHarness harness = new UserTestHarness(nameof(StartRegister_ShouldReturnBadRequest_WhenBodyNull));

            var result = await harness.Controller.StartRegister(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task VerifyRegister_ShouldReturnBadRequest_WhenBodyNull()
        {
            UserTestHarness harness = new UserTestHarness(nameof(VerifyRegister_ShouldReturnBadRequest_WhenBodyNull));

            var result = await harness.Controller.VerifyRegister(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenBodyNull()
        {
            UserTestHarness harness = new UserTestHarness(nameof(Login_ShouldReturnBadRequest_WhenBodyNull));

            var result = await harness.Controller.Login(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ---------------- invalid password test ----------------
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenWrongPassword()
        {
            UserTestHarness harness = new UserTestHarness(nameof(Login_ShouldReturnUnauthorized_WhenWrongPassword));

            await harness.SeedUserAsync("john", "Password123!", "+3161000000");

            var result = await harness.Controller.Login(new LoginDto
            {
                Identifier = "john",
                Password = "WrongPassword!"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // ---------------- verification code invalid test ----------------
        [Fact]
        public async Task VerifyRegister_ShouldReturnUnauthorizedOrBadRequest_WhenCodeWrong()
        {
            UserTestHarness harness = new UserTestHarness(nameof(VerifyRegister_ShouldReturnUnauthorizedOrBadRequest_WhenCodeWrong));

            await harness.Controller.StartRegister(new RegisterDto
            {
                Username = "john",
                Password = "Password123!",
                Email = "john@test.com",
                PhoneNumber = "0612345678"
            });

            var v = harness.Db.UserVerifications.FirstOrDefault();
            Assert.NotNull(v);

            var result = await harness.Controller.VerifyRegister(new VerifyUserDto{
                PhoneNumber = v!.PhoneNumber,
                Code = "000000"
            });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // ---------------- Authenticated user tests ----------------
        [Fact]
        public void ReturnJwt_ShouldReturnOk_WithClaims()
        {
            UserTestHarness harness = new UserTestHarness(nameof(ReturnJwt_ShouldReturnOk_WithClaims));
            harness.SetAuthenticatedUser(5, "john");

            var result = harness.Controller.Verify();

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<UserDto>(ok.Value);

            Assert.Equal(5, dto.Id);
            Assert.Equal("john", dto.Username);
        }

        [Fact]
        public async Task GetUserInfo_ShouldReturnOk_WhenUserExists()
        {
            UserTestHarness harness = new UserTestHarness(nameof(GetUserInfo_ShouldReturnOk_WhenUserExists));

            var seeded = await harness.SeedUserAsync("john", "Password123!", "+3161000000");
            harness.SetAuthenticatedUser(seeded.Id, seeded.Username);

            var result = await harness.Controller.GetUserInfo();

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<UserDto>(ok.Value);

            Assert.Equal(seeded.Id, dto.Id);
            Assert.Equal("john", dto.Username);
        }

        [Fact]
        public async Task GetUserInfo_ShouldReturnNotFound_WhenUserMissing()
        {
            UserTestHarness harness = new UserTestHarness(nameof(GetUserInfo_ShouldReturnNotFound_WhenUserMissing));

            harness.SetAuthenticatedUser(999, "ghost");

            var result = await harness.Controller.GetUserInfo();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void Logout_ShouldReturnOk()
        {
            UserTestHarness harness = new UserTestHarness(nameof(Logout_ShouldReturnOk));
            harness.SetAuthenticatedUser(1, "john");

            var result = harness.Controller.Logout();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Logged out", ok.Value);
        }
    }
}


