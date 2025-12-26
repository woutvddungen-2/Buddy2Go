using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Common;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Server.Tests.TestUtilities;
using Shared.Models.Dtos.Users;
using Shared.Models.Enums;

namespace Server.Tests.Users.Unit
{
    public class UserServiceTests
    {
        //--------------- StartRegister --------------------------

        [Fact]
        public async Task Register_ShouldFail_WhenMissingFields()
        {
            UserService? service = CreateService();

            ServiceResult? result = await service.StartRegistrationAsync("", "Password1234!", "Test@Test.com", "06-12121212");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);

            result = await service.StartRegistrationAsync("Test", "", "Test@Test.com", "06-12121212");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);

            result = await service.StartRegistrationAsync("Test", "Password1234!", "", "06-12121212");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);

            result = await service.StartRegistrationAsync("Test", "Password1234!", "Test@Test.com", "");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);
        }

        [Fact]
        public async Task Register_ShouldFail_WhenWeakPassword()
        {
            UserService? service = CreateService();

            ServiceResult? result = await service.StartRegistrationAsync("Test", "Password1234", "Test@Test.com", "06-12121212");

            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);
        }

        //--------------- CompleteRegister --------------------------
        [Fact]
        public async Task CompleteRegistration_ShouldFail_WhenCodeNotFound()
        {
            UserService service = CreateService();

            var result = await service.CompleteRegistrationAsync("+31612345678", "000000");

            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ResourceNotFound, result.Status);
        }

        [Fact]
        public async Task CompleteRegistration_ShouldFail_WhenCodeExpired()
        {
            UserService service = CreateService(out AppDbContext db);

            db.UserVerifications.Add(new UserVerification
            {
                PhoneNumber = "+31612345678",
                Code = "123456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // expired
                Type = VerificationType.Registration
            });

            db.SaveChanges();

            var result = await service.CompleteRegistrationAsync("+31612345678", "123456");

            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);
        }

        [Fact]
        public async Task CompleteRegistration_ShouldFail_WhenCodeIncorrect()
        {
            UserService service = CreateService(out AppDbContext db);

            db.UserVerifications.Add(new UserVerification
            {
                PhoneNumber = "+31612345678",
                Code = "123456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Type = VerificationType.Registration
            });

            db.SaveChanges();

            var result = await service.CompleteRegistrationAsync("+31612345678", "999999");

            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);
        }

        [Fact]
        public async Task CompleteRegistration_ShouldSucceed()
        {
            UserService service = CreateService(out AppDbContext db);

            db.UserVerifications.Add(new UserVerification
            {
                PhoneNumber = "+31612345678",
                Username = "Test",
                PasswordHash = UserService.HashPassword("Password1234!"),
                Email = "test@test.com",
                Code = "123456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Type = VerificationType.Registration
            });

            db.SaveChanges();

            var result = await service.CompleteRegistrationAsync("+31612345678", "123456");

            Assert.True(result.IsSuccess);

            var user = db.Users.FirstOrDefault();
            Assert.NotNull(user);
            Assert.Equal("Test", user!.Username);
        }

        //--------------- Login --------------------------

        [Fact]
        public async Task Login_ShouldFail_WhenUserDoesNotExist()
        {
            UserService? service = CreateService();

            ServiceResult<string>? result = await service.Login("ghost", "abc");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);
        }


        [Fact]
        public async Task Login_ShouldFail_WhenIncorrectCredentials()
        {
            UserService service = CreateService(out AppDbContext db);
            await AddTestUser(db, id: 1);

            ServiceResult<string>? result = await service.Login("Test", "Password123!");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);

            result = await service.Login("", "Password123!");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);

            result = await service.Login("Test", "");
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);
        }

        [Fact]
        public async Task Login_ShouldSucceed()
        {
            UserService service = CreateService(out AppDbContext db);
            await AddTestUser(db, id: 1);

            ServiceResult<string>? result = await service.Login("Test", "Password1234!");
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
        }

        // -------------- GetUserInfo -----------------------

        [Fact]
        public async Task GetuserInfo_ShouldFail_UsernNotFound()
        {
            UserService? service = CreateService();

            ServiceResult<UserDto>? result = await service.GetUserInfo(1);
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.UserNotFound, result.Status);
        }

        [Fact]
        public async Task GetuserInfo_ShouldSucceed()
        {
            UserService service = CreateService(out AppDbContext db);
            await AddTestUser(db, id: 1);

            ServiceResult<UserDto> result = await service.GetUserInfo(1);
            Assert.True(result.IsSuccess);
            Assert.Equal("Test", result.Data?.Username);
            Assert.Equal(1, result.Data?.Id);
        }

        // ---------------- FindUserbyPhone --------------------

        [Fact]
        public async Task FindUserbyPhone_ShouldFail_UserNotFound()
        {
            UserService? service = CreateService();

            ServiceResult<UserDto>? result = await service.FindUserbyPhone("06-12345678", 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.UserNotFound, result.Status);
        }

        [Fact]
        public async Task FindUserbyPhone_ShouldFail_UnusablePhonenumber()
        {
            UserService? service = CreateService();

            ServiceResult<UserDto>? result = await service.FindUserbyPhone("", 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);

            result = await service.FindUserbyPhone("06-1234", 1);
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultStatus.ValidationError, result.Status);
        }

        [Fact]
        public async Task FindUserbyPhone_ShouldSucceed()
        {
            UserService service = CreateService(out AppDbContext db);
            await AddTestUser(db, id: 1);

            ServiceResult<UserDto>? result = await service.FindUserbyPhone("0611111111", 1);
            Assert.True(result.IsSuccess);
            Assert.Equal("Test", result.Data?.Username);
            Assert.Equal("Test@Test.com", result.Data?.Email);
            Assert.Equal("0611111111", result.Data?.PhoneNumber);
        }

        // ------------- helpers ---------------
        private static UserService CreateService()
        {
            return CreateService(out _);
        }
        private static UserService CreateService(out AppDbContext db)
        {
            db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());

            IConfiguration config = CreateConfig();

            var logger = Mock.Of<ILogger<UserService>>();

            var smsMock = new Mock<ISmsService>();
            smsMock.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("OK");
            return new UserService(db, logger, config, smsMock.Object);
        }
        private static IConfiguration CreateConfig()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Secret", "ThisIsASuperSecretKey12345678901" }
                })
                .Build();
        }
        private async Task<User> AddTestUser(AppDbContext db, int id = 1)
        {
            User user = new User
            {
                Id = id,
                Username = "Test",
                PasswordHash = UserService.HashPassword("Password1234!"),
                Email = "Test@Test.com",
                Phonenumber = "0611111111",
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

    }
}
