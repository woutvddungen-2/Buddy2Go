using Microsoft.AspNetCore.Mvc;
using Moq;
using Server.Features.Users;
using Shared.Models.Dtos.Users;

namespace Server.Tests.Users.Integration
{
    public class UserIntegrationTests
    {

        [Fact]
        public async Task FullRegistrationTest()
        {
            // ───────────────────────────────────────────────────────────────
            // Arrange: Setup InMemory DB + Mock SMS service
            // ───────────────────────────────────────────────────────────────
            var harness = UserTestHarness.Create(nameof(FullRegistrationTest));


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

            IActionResult startResult = await harness.Controller.StartRegister(dto);

            Assert.IsType<OkObjectResult>(startResult);

            // Verify SMS was triggered
            harness.SmsMock.Verify(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // ───────────────────────────────────────────────────────────────
            // Step 2: Retrieve verification code from DB
            // ───────────────────────────────────────────────────────────────
            UserVerification? verification = harness.Db.UserVerifications.FirstOrDefault();
            Assert.NotNull(verification);

            string code = verification!.Code;
            string normalizedPhone = verification!.PhoneNumber;

            // ───────────────────────────────────────────────────────────────
            // Step 3: Call VerifyRegister
            // ───────────────────────────────────────────────────────────────
            VerifyUserDto verifyDto = new VerifyUserDto
            {
                PhoneNumber = normalizedPhone,
                Code = code
            };

            IActionResult verifyResult = await harness.Controller.VerifyRegister(verifyDto);

            Assert.IsType<OkObjectResult>(verifyResult);

            // ───────────────────────────────────────────────────────────────
            // Step 4: Verify user was created in DB
            // ───────────────────────────────────────────────────────────────
            User? user = harness.Db.Users.FirstOrDefault(u => u.Username == "john");

            Assert.NotNull(user);
            Assert.Equal(normalizedPhone, user!.Phonenumber);
            Assert.Equal(dto.Email, user!.Email);
            Assert.Equal(dto.Username, user!.Username);
        }

        [Fact]
        public async Task FullLoginTest()
        {
            // Setup: Seed a user in the InMemory DB
            var harness = UserTestHarness.Create(nameof(FullLoginTest));
            User Seeduser = await harness.SeedUserAsync();

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
    }
}


