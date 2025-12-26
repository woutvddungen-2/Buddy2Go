using Server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models.Dtos.Users;
using System.Net;
using System.Net.Http.Json;
using Server.Tests.TestUtilities;

namespace Server.Tests.Users.Functional
{
    public class UserFunctionalTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory factory;
        private readonly HttpClient client;

        public UserFunctionalTests(CustomWebAppFactory factory)
        {
            this.factory = factory;

            using var scope = this.factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Keep cookies (jwt) if your API sets them
            client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            });
        }

        [Fact]
        public async Task Register_Verify_Login_FullFlow_ViaHttp()
        {
            // ----------------------------
            // 1) StartRegister via HTTP
            // ----------------------------
            var registerDto = new RegisterDto
            {
                Username = "john",
                Password = "Password123!",
                Email = "john@test.com",
                PhoneNumber = "0612345678"
            };

            var startRes = await client.PostAsJsonAsync("/api/User/StartRegister", registerDto);
            Assert.Equal(HttpStatusCode.OK, startRes.StatusCode);

            // ----------------------------
            // 2) Fetch verification code from DB (same in-memory DB as server)
            // ----------------------------
            string phone;
            string code;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var verification = await db.UserVerifications.AsNoTracking().FirstOrDefaultAsync();
                Assert.NotNull(verification);

                phone = verification!.PhoneNumber;
                code = verification.Code;
            }

            // ----------------------------
            // 3) VerifyRegister via HTTP
            // ----------------------------
            var verifyDto = new VerifyUserDto
            {
                PhoneNumber = phone,
                Code = code
            };

            var verifyRes = await client.PostAsJsonAsync("/api/User/VerifyRegister", verifyDto);
            Assert.Equal(HttpStatusCode.OK, verifyRes.StatusCode);

            // ----------------------------
            // 4) Login via HTTP
            // ----------------------------
            var loginDto = new LoginDto
            {
                Identifier = "john",
                Password = "Password123!"
            };

            var loginRes = await client.PostAsJsonAsync("/api/User/Login", loginDto);
            Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

            // Expect { token = "..." }
            var payload = await loginRes.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(payload);
            Assert.True(payload!.TryGetValue("token", out var token));
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenWrongPassword_ViaHttp()
        {
            // Seed user by running only the minimal required flow:
            // StartRegister -> VerifyRegister (same as above, but short)
            var registerDto = new RegisterDto
            {
                Username = "john",
                Password = "Password123!",
                Email = "john@test.com",
                PhoneNumber = "0612345678"
            };

            var startRes = await client.PostAsJsonAsync("/api/User/StartRegister", registerDto);
            Assert.Equal(HttpStatusCode.OK, startRes.StatusCode);

            string phone;
            string code;

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var verification = await db.UserVerifications.AsNoTracking().FirstAsync();
                phone = verification.PhoneNumber;
                code = verification.Code;
            }

            var verifyRes = await client.PostAsJsonAsync("/api/User/VerifyRegister", new VerifyUserDto
            {
                PhoneNumber = phone,
                Code = code
            });
            Assert.Equal(HttpStatusCode.OK, verifyRes.StatusCode);

            // Wrong password
            var loginRes = await client.PostAsJsonAsync("/api/User/Login", new LoginDto
            {
                Identifier = "john",
                Password = "WrongPassword123!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, loginRes.StatusCode);
        }
    }
}