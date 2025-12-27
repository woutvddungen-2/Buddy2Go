using Microsoft.AspNetCore.Http;
using Server.Features.Users;
using Server.Tests.TestUtilities;
using Shared.Models.Dtos.Buddies;
using Shared.Models.Dtos.Shared;
using Shared.Models.enums;
using System.Net;
using System.Net.Http.Json;

namespace Server.Tests.Buddies.Functional
{
    public class BuddyFunctionalTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory factory;

        public BuddyFunctionalTests(CustomWebAppFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task SendBuddyRequest_ShouldReturnOk_AndAppearInPendingAndSendLists()
        {
            BuddyFunctionalHarness harness = new BuddyFunctionalHarness(factory);

            User john = await harness.CreateUserAsync("john", "Password123!", "+3161000000");
            User jane = await harness.CreateUserAsync("jane", "Password123!", "+3162000000");

            HttpClient johnClient = await harness.LoginAsAsync(john.Email, "Password123!");
            HttpClient janeClient = await harness.LoginAsAsync(jane.Email, "Password123!");

            // send request john -> jane
            HttpResponseMessage sendRes = await johnClient.PostAsync($"/api/Buddy/Send/{jane.Id}", content: null);
            Assert.Equal(HttpStatusCode.OK, sendRes.StatusCode);

            // requester should see request
            HttpResponseMessage sendListRes = await johnClient.GetAsync("/api/Buddy/GetSend");
            Assert.Equal(HttpStatusCode.OK, sendListRes.StatusCode);

            List<BuddyDto>? sendList = await sendListRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(sendList);
            Assert.Contains(sendList, b => b.Requester.Id == john.Id && b.Addressee.Id == jane.Id && b.Status == RequestStatus.Pending);

            // Now jane can verify if it shows up
            HttpResponseMessage pendingRes = await janeClient.GetAsync("/api/Buddy/Pending");
            Assert.Equal(HttpStatusCode.OK, pendingRes.StatusCode);

            List<BuddyDto>? pending = await pendingRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(pending);
            Assert.Contains(pending, b => b.Requester.Id == john.Id && b.Addressee.Id == jane.Id && b.Status == RequestStatus.Pending);
        }

        [Fact]
        public async Task RespondToRequest_Accept_ShouldMoveToList_ForBothUsers()
        {
            BuddyFunctionalHarness harness = new BuddyFunctionalHarness(factory);

            User john = await harness.CreateUserAsync("john", "Password123!", "+3161000000");
            User jane = await harness.CreateUserAsync("jane", "Password123!", "+3162000000");

            HttpClient johnClient = await harness.LoginAsAsync(john.Email, "Password123!");
            HttpClient janeClient = await harness.LoginAsAsync(jane.Email, "Password123!");

            // john sends request
            HttpResponseMessage sendRes = await johnClient.PostAsync($"/api/Buddy/Send/{jane.Id}", null);
            Assert.Equal(HttpStatusCode.OK, sendRes.StatusCode);

            // jane accepts
            HttpResponseMessage respondRes = await janeClient.PatchAsJsonAsync("/api/Buddy/Respond", new RequestResponseDto
            {
                RequesterId = john.Id,
                Status = RequestStatus.Accepted
            });
            Assert.Equal(HttpStatusCode.OK, respondRes.StatusCode);

            // both should now see accepted buddy in List
            HttpResponseMessage johnListRes = await johnClient.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, johnListRes.StatusCode);
            List<BuddyDto>? johnList = await johnListRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(johnList);
            Assert.Contains(johnList, b => (b.Requester.Id == john.Id && b.Addressee.Id == jane.Id) || (b.Requester.Id == jane.Id && b.Addressee.Id == john.Id));

            HttpResponseMessage janeListRes = await janeClient.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, janeListRes.StatusCode);
            List<BuddyDto>? janeList = await janeListRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(janeList);
            Assert.Contains(janeList, b => (b.Requester.Id == john.Id && b.Addressee.Id == jane.Id) || (b.Requester.Id == jane.Id && b.Addressee.Id == john.Id));
        }

        [Fact]
        public async Task SendBuddyRequest_ShouldReturnBadRequest_WhenSendingToSelf()
        {
            BuddyFunctionalHarness harness = new BuddyFunctionalHarness(factory);
            User john = await harness.CreateUserAsync("john", "Password123!", "+3161000000");

            HttpClient johnClient = await harness.LoginAsAsync("john", "Password123!");

            HttpResponseMessage res = await johnClient.PostAsync($"/api/Buddy/Send/{john.Id}", null);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task BlockBuddy_ShouldBlockAcceptedBuddy_AndRemoveFromList()
        {
            BuddyFunctionalHarness harness = new BuddyFunctionalHarness(factory);

            User john = await harness.CreateUserAsync("john", "Password123!", "+3161000000");
            User jane = await harness.CreateUserAsync("jane", "Password123!", "+3162000000");

            HttpClient johnClient = await harness.LoginAsAsync(john.Email, "Password123!");
            HttpClient janeClient = await harness.LoginAsAsync(jane.Email, "Password123!");

            // Create accepted buddy relation via API
            HttpResponseMessage Sendres = await johnClient.PostAsync($"/api/Buddy/Send/{jane.Id}", null);
            Assert.Equal(HttpStatusCode.OK, Sendres.StatusCode);

            HttpResponseMessage ResponseRes = await janeClient.PatchAsJsonAsync("/api/Buddy/Respond", new RequestResponseDto
            {
                RequesterId = john.Id,
                Status = RequestStatus.Accepted
            });
            Assert.Equal(HttpStatusCode.OK, ResponseRes.StatusCode);

            // jane blocks john
            HttpResponseMessage blockRes = await janeClient.PatchAsync($"/api/Buddy/Block/{john.Id}", null);
            Assert.Equal(HttpStatusCode.OK, blockRes.StatusCode);

            // list should not show accepted buddy anymore
            HttpResponseMessage listRes = await janeClient.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);

            List<BuddyDto>? list = await listRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(list);
            Assert.DoesNotContain(list!, b =>
                (b.Requester.Id == john.Id && b.Addressee.Id == jane.Id) ||
                (b.Requester.Id == jane.Id && b.Addressee.Id == john.Id));

            // john tries to send buddy request again and gets blocked response
            HttpResponseMessage resendRes = await johnClient.PostAsync($"/api/Buddy/Send/{jane.Id}", null);
            Assert.Equal(HttpStatusCode.BadRequest, resendRes.StatusCode);
        }

        [Fact]
        public async Task DeleteBuddy_ShouldRemoveAcceptedBuddy_AndRemoveFromList()
        {
            BuddyFunctionalHarness harness = new BuddyFunctionalHarness(factory);

            User john = await harness.CreateUserAsync("john", "Password123!", "+3161000000");
            User jane = await harness.CreateUserAsync("jane", "Password123!", "+3162000000");

            HttpClient johnClient = await harness.LoginAsAsync(john.Email, "Password123!");
            HttpClient janeClient = await harness.LoginAsAsync(jane.Email, "Password123!");

            // Create accepted buddy relation
            HttpResponseMessage Sendres = await johnClient.PostAsync($"/api/Buddy/Send/{jane.Id}", null);
            Assert.Equal(HttpStatusCode.OK, Sendres.StatusCode);

            // jane accepts
            HttpResponseMessage respondRes = await janeClient.PatchAsJsonAsync("/api/Buddy/Respond", new RequestResponseDto
            {
                RequesterId = john.Id,
                Status = RequestStatus.Accepted
            });
            Assert.Equal(HttpStatusCode.OK, respondRes.StatusCode);

            // jane deletes buddy
            HttpResponseMessage delRes = await janeClient.DeleteAsync($"/api/Buddy/Delete/{john.Id}");
            Assert.Equal(HttpStatusCode.OK, delRes.StatusCode);

            // list empty for Jane
            HttpResponseMessage listJaneRes = await janeClient.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, listJaneRes.StatusCode);
            List<BuddyDto>? JaneList = await listJaneRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(JaneList);
            Assert.Empty(JaneList);

            // list empty for John
            HttpResponseMessage listJohnRes = await johnClient.GetAsync("/api/Buddy/List");
            Assert.Equal(HttpStatusCode.OK, listJohnRes.StatusCode);
            List<BuddyDto>? Johnlist = await listJohnRes.Content.ReadFromJsonAsync<List<BuddyDto>>();
            Assert.NotNull(Johnlist);
            Assert.Empty(Johnlist);
        }
    }
}
