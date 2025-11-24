using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Shared.Models;

namespace Server.Helpers
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, ILogger logger)
        {
            // Apply migrations (safe even if DB already exists)
            await context.Database.MigrateAsync();

#if DEBUG
            logger.LogInformation("DEBUG mode: Clearing data...");

            // Just clear data, not schema
            await context.JourneyMessages.ExecuteDeleteAsync();
            await context.JourneyParticipants.ExecuteDeleteAsync();
            await context.Buddys.ExecuteDeleteAsync();
            await context.DangerousPlaces.ExecuteDeleteAsync();
            await context.Journeys.ExecuteDeleteAsync();
            await context.Users.ExecuteDeleteAsync();

            logger.LogInformation("DEBUG mode: Seeding data...");
            await SeedDataAsync(context);
#endif
        }

        private static async Task SeedDataAsync(AppDbContext context)
        {
            // ----------- Users -----------
            var users = new List<User>
            {
                new User { Id = 1, Username = "Alice", Email = "alice@test.com", Phonenumber = "0600000000", PasswordHash = "Btd5kOga0bCQboFgEC27wQXmHO/7+ycka95ivGi4EXXAEOj303ehnFqmaGr3+rHi", CreatedAt = DateTime.UtcNow },
                new User { Id = 2, Username = "Bob", Email = "bob@test.com", Phonenumber = "0611111111", PasswordHash = "DbjdjPrHA2CdSDtuDrpWqWbAcxPQIoxHxNz73a0P8CFWd/Sg55yo/+FTDbdsxtdL", CreatedAt = DateTime.UtcNow },
                new User { Id = 3, Username = "Charlie", Email = "charlie@test.com", Phonenumber = "0622222222", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.UtcNow },
                new User { Id = 4, Username = "Joseph", Email = "Joseph@test.com", Phonenumber = "0633333333", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.UtcNow },
                new User { Id = 5, Username = "Diana", Email = "Diana@test.com", Phonenumber = "0644444444", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.UtcNow }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // ----------- Journeys -----------
            var journeys = new List<Journey>
            {
                new Journey { Id = 1, StartId = 1, EndId = 16, CreatedAt = DateTime.UtcNow, FinishedAt = null },
                new Journey { Id = 2, StartId = 2, EndId = 7, CreatedAt = DateTime.UtcNow, FinishedAt = null },
                new Journey { Id = 3, StartId = 12, EndId = 10, CreatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), FinishedAt = null },
                new Journey { Id = 4, StartId = 20, EndId = 32, CreatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)), FinishedAt = DateTime.UtcNow }
            };
            await context.Journeys.AddRangeAsync(journeys);
            await context.SaveChangesAsync();

            // ----------- JourneyParticipants -----------
            var participants = new List<JourneyParticipant>
            {
                new JourneyParticipant { UserId = 1, JourneyId = 1, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow },
                new JourneyParticipant { UserId = 2, JourneyId = 1, Role = JourneyRole.Participant, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow },
                new JourneyParticipant { UserId = 3, JourneyId = 1, Role = JourneyRole.Participant, Status = RequestStatus.Pending, JoinedAt = DateTime.UtcNow },
                new JourneyParticipant { UserId = 4, JourneyId = 1, Role = JourneyRole.Participant, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow },

                new JourneyParticipant { UserId = 2, JourneyId = 2, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow },
                new JourneyParticipant { UserId = 1, JourneyId = 2, Role = JourneyRole.Participant, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow },
                new JourneyParticipant { UserId = 4, JourneyId = 2, Role = JourneyRole.Participant, Status = RequestStatus.Pending, JoinedAt = DateTime.UtcNow },
                new JourneyParticipant { UserId = 5, JourneyId = 2, Role = JourneyRole.Participant, Status = RequestStatus.Rejected, JoinedAt = DateTime.UtcNow },

                new JourneyParticipant { UserId = 4, JourneyId = 3, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(12)) },
                new JourneyParticipant { UserId = 4, JourneyId = 4, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)) }
            };
            await context.JourneyParticipants.AddRangeAsync(participants);
            await context.SaveChangesAsync();

            // ----------- JourneyMessages -----------
            var messages = new List<JourneyMessage>
            {
                new JourneyMessage { Id = 1, JourneyId = 1, SenderId = 1, Content = "Hi Bob!", SentAt = DateTime.UtcNow },
                new JourneyMessage { Id = 2, JourneyId = 1, SenderId = 2, Content = "Hey Alice!", SentAt = DateTime.UtcNow },
                new JourneyMessage { Id = 3, JourneyId = 1, SenderId = 2, Content = "Hello Charlie!", SentAt = DateTime.UtcNow },

                new JourneyMessage { Id = 4, JourneyId = 2, SenderId = 1, Content = "Hi Bob!", SentAt = DateTime.UtcNow },
                new JourneyMessage { Id = 5, JourneyId = 2, SenderId = 2, Content = "Hey Alice!", SentAt = DateTime.UtcNow },
                new JourneyMessage { Id = 6, JourneyId = 2, SenderId = 2, Content = "Hello Charlie!", SentAt = DateTime.UtcNow }
            };
            await context.JourneyMessages.AddRangeAsync(messages);
            await context.SaveChangesAsync();

            // ----------- DangerousPlace -----------
            var dangerousPlaces = new List<DangerousPlace>
            {
                new DangerousPlace { Id = 1, ReportedById = 1, GPS = "52.370216,4.895168", PlaceType = DangerousPlaceType.PoorLighting, Description = "Very dark street, watch out!" },
                new DangerousPlace { Id = 2, ReportedById = 2, GPS = "51.924420,4.477733", PlaceType = DangerousPlaceType.CrimeSpot, Description = "Lots of garbage here" }
            };
            await context.DangerousPlaces.AddRangeAsync(dangerousPlaces);
            await context.SaveChangesAsync();

            // ----------- Buddys -----------
            var buddies = new List<Buddy>
            {
                new Buddy { RequesterId = 1, AddresseeId = 2, Status = RequestStatus.Accepted },
                new Buddy { RequesterId = 1, AddresseeId = 3, Status = RequestStatus.Accepted },
                new Buddy { RequesterId = 4, AddresseeId = 1, Status = RequestStatus.Accepted },

                new Buddy { RequesterId = 2, AddresseeId = 4, Status = RequestStatus.Accepted },
                new Buddy { RequesterId = 5, AddresseeId = 2, Status = RequestStatus.Accepted },

                new Buddy { RequesterId = 3, AddresseeId = 4, Status = RequestStatus.Pending }
            };
            await context.Buddys.AddRangeAsync(buddies);
            await context.SaveChangesAsync();
        }
    }
}
