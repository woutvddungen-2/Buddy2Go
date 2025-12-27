using Microsoft.Extensions.DependencyInjection;
using Server.Infrastructure.Data;
using Server.Tests.TestUtilities;
using Server.Features.Users;
using Shared.Models.Dtos.Buddies;
using Shared.Models.Dtos.Shared;
using Shared.Models.Dtos.Users;
using Shared.Models.enums;
using System.Net;
using System.Net.Http.Json;

namespace Server.Tests.Buddies.Functional
{
    public class BuddyFunctionalTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory factory;
        private readonly HttpClient client;

        public BuddyFunctionalTests(CustomWebAppFactory factory)
        {
            this.factory = factory;

            using var scope = this.factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Keep cookies
            client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            });
        }
        [Fact]
        public async Task SendBuddyRequest_ShouldReturnOk_AndAppearInPendingAndSendLists()
        {
            int johnId;
            int janeId;

            using (var scope = factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                UserDto john = await SeedUserAsync(db, "john", "Password123!", "+3161000000");
                UserDto jane = await SeedUserAsync(db, "jane", "Password123!", "+3162000000");
                johnId = john.Id;
                janeId = jane.Id;
            }


            await LoginAsAsync("john", "Password123!");

            // send request john -> jane
            HttpResponseMessage sendRes = await client.PostAsync($"/api/Buddy/Send/{janeId}", content: null);
            Assert.Equal(HttpStatusCode.OK, sendRes.StatusCode);

            // requester should see request
            HttpResponseMessage sendListRes = await client.GetAsync("/api/Buddy/GetSend");
            Assert.Equal(HttpStatusCode.OK, sendListRes.StatusCode);

            List<BuddyDto>? sendList = await sendListRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(sendList);
            Assert.Contains(sendList, b => b.Requester.Id == johnId && b.Addressee.Id == janeId && b.Status == RequestStatus.Pending);

            // Now login as jane and verify it shows up
            await LoginAsAsync("jane", "Password123!");

            HttpResponseMessage pendingRes = await client.GetAsync("/api/Buddy/Pending");
            Assert.Equal(HttpStatusCode.OK, pendingRes.StatusCode);

            List<BuddyDto>? pending = await pendingRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(pending);
            Assert.Contains(pending, b => b.Requester.Id == johnId && b.Addressee.Id == janeId && b.Status == RequestStatus.Pending);
        }

        [Fact]
        public async Task RespondToRequest_Accept_ShouldMoveToList_ForBothUsers()
        {
            int johnId;
            int janeId;

            using (var scope = factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                UserDto john = await SeedUserAsync(db, "john", "Password123!", "+3161000000");
                UserDto jane = await SeedUserAsync(db, "jane", "Password123!", "+3162000000");
                johnId = john.Id;
                janeId = jane.Id;
            }

            // john sends request
            await LoginAsAsync("john", "Password123!");
            HttpResponseMessage sendRes = await client.PostAsync($"/api/Buddy/Send/{janeId}", null);
            Assert.Equal(HttpStatusCode.OK, sendRes.StatusCode);

            // jane accepts
            await LoginAsAsync("jane", "Password123!");
            HttpResponseMessage respondRes = await client.PatchAsJsonAsync("/api/Buddy/Respond", new RequestResponseDto
            {
                RequesterId = johnId,
                Status = RequestStatus.Accepted
            });
            Assert.Equal(HttpStatusCode.OK, respondRes.StatusCode);

            // both should now see accepted buddy in List
            HttpResponseMessage janeListRes = await client.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, janeListRes.StatusCode);
            List<BuddyDto>? janeList = await janeListRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(janeList);
            Assert.Contains(janeList!, b => (b.Requester.Id == johnId && b.Addressee.Id == janeId) || (b.Requester.Id == janeId && b.Addressee.Id == johnId));

            await LoginAsAsync("john", "Password123!");
            HttpResponseMessage johnListRes = await client.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, johnListRes.StatusCode);
            List<BuddyDto>? johnList = await johnListRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(johnList);
            Assert.Contains(johnList!, b => (b.Requester.Id == johnId && b.Addressee.Id == janeId) || (b.Requester.Id == janeId && b.Addressee.Id == johnId));
        }

        [Fact]
        public async Task SendBuddyRequest_ShouldReturnBadRequest_WhenSendingToSelf()
        {
            using (var scope = factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await SeedUserAsync(db, "john", "Password123!", "+3161000000");
            }

            await LoginAsAsync("john", "Password123!");

            HttpResponseMessage res = await client.PostAsync("/api/Buddy/Send/1", null);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task BlockBuddy_ShouldBlockAcceptedBuddy_AndRemoveFromList()
        {
            int johnId;
            int janeId;
            using (var scope = factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                UserDto john = await SeedUserAsync(db, "john", "Password123!", "+3161000000");
                UserDto jane = await SeedUserAsync(db, "jane", "Password123!", "+3162000000");
                johnId = john.Id;
                janeId = jane.Id;
            }

            // Create accepted buddy relation via API
            await LoginAsAsync("john", "Password123!");
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/Buddy/Send/{janeId}", null)).StatusCode);

            await LoginAsAsync("jane", "Password123!");
            Assert.Equal(HttpStatusCode.OK,
                (await client.PatchAsJsonAsync("/api/Buddy/Respond", new RequestResponseDto
                {
                    RequesterId = johnId,
                    Status = RequestStatus.Accepted
                })).StatusCode);

            // jane blocks john
            HttpResponseMessage blockRes = await client.PatchAsync($"/api/Buddy/Block/{johnId}", content: null);
            Assert.Equal(HttpStatusCode.OK, blockRes.StatusCode);

            // list should not show accepted buddy anymore
            HttpResponseMessage listRes = await client.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);

            List<BuddyDto>? list = await listRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(list);
            Assert.DoesNotContain(list!, b =>
                (b.Requester.Id == johnId && b.Addressee.Id == janeId) ||
                (b.Requester.Id == janeId && b.Addressee.Id == johnId));
        }

        [Fact]
        public async Task DeleteBuddy_ShouldRemoveAcceptedBuddy_AndRemoveFromList()
        {
            int johnId;
            int janeId;
            using (var scope = factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                UserDto john = await SeedUserAsync(db, "john", "Password123!", "+3161000000");
                UserDto jane = await SeedUserAsync(db, "jane", "Password123!", "+3162000000");
                johnId = john.Id;
                janeId = jane.Id;
            }

            // Create accepted buddy relation
            await LoginAsAsync("john", "Password123!");
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/Buddy/Send/{janeId}", null)).StatusCode);

            await LoginAsAsync("jane", "Password123!");
            Assert.Equal(HttpStatusCode.OK,
                (await client.PatchAsJsonAsync("/api/Buddy/Respond", new RequestResponseDto
                {
                    RequesterId = johnId,
                    Status = RequestStatus.Accepted
                })).StatusCode);

            // jane deletes buddy
            HttpResponseMessage delRes = await client.DeleteAsync($"/api/Buddy/Delete/{johnId}");
            Assert.Equal(HttpStatusCode.OK, delRes.StatusCode);

            // list empty
            HttpResponseMessage listRes = await client.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);

            List<BuddyDto>? list = await listRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        //------------------ Helpers ------------------
        private async Task LoginAsAsync(string identifier, string password)
        {
            // ensure previous cookie doesn't keep an old identity
            var opts = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            };

            // Replace client so cookie jar resets
            client.Dispose();
            HttpClient newClient = factory.CreateClient(opts);
            typeof(BuddyFunctionalTests)
                .GetField("client", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(this, newClient);

            HttpResponseMessage? loginRes = await newClient.PostAsJsonAsync("/api/User/Login", new LoginDto
            {
                Identifier = identifier,
                Password = password
            });

            Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
        }

        private static async Task<UserDto> SeedUserAsync(AppDbContext db, string username, string password, string phonenumber)
        {
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
            return new UserDto 
            {
                Id = user.Id, 
                Username = user.Username ,
                Email = user.Email, 
                PhoneNumber = user.Phonenumber, 
                CreatedAt = user.CreatedAt
            };
        }
    }
}
