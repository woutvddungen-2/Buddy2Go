using Microsoft.Extensions.DependencyInjection;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Server.Tests.TestUtilities;
using Shared.Models.Dtos.Users;
using System.Net;
using System.Net.Http.Json;

namespace Server.Tests.Buddies.Functional
{
    public class BuddyFunctionalHarness
    {
        private readonly CustomWebAppFactory factory;

        public BuddyFunctionalHarness(CustomWebAppFactory factory)
        {
            this.factory = factory;

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        public async Task<HttpClient> LoginAsAsync(string identifier, string password)
        {
            var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            });

            HttpResponseMessage? loginRes = await client.PostAsJsonAsync("/api/User/Login", new LoginDto
            {
                Identifier = identifier,
                Password = password
            });

            if (loginRes.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Login failed in harness");

            return client;
        }

        public async Task<User> CreateUserAsync(string username, string password, string phonenumber)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            User user = new User
            {
                Username = username,
                PasswordHash = UserService.HashPassword(password),
                Email = $"{username}@test.com",
                Phonenumber = phonenumber,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }
    }
}
