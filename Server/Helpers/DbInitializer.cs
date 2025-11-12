using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Shared.Models;

namespace Server.Helpers
{
    public static class DbInitializer
    {
        public static async Task ResetDatabaseAsync(AppDbContext context)
        {
#if DEBUG
            // Ensure database exists and apply migrations
            await context.Database.MigrateAsync();

            // Remove data in correct FK order
            if (await context.Users.AnyAsync())
            {
                await context.Messages.ExecuteDeleteAsync();
                await context.JourneyParticipants.ExecuteDeleteAsync();
                await context.Buddys.ExecuteDeleteAsync();
                await context.DangerousPlaces.ExecuteDeleteAsync();
                await context.Journeys.ExecuteDeleteAsync();
                await context.Users.ExecuteDeleteAsync();
                await context.Places.ExecuteDeleteAsync();
            }

            // Seed fresh data
            await SeedDataAsync(context);
#endif
        }

        private static async Task SeedDataAsync(AppDbContext context)
        {
            // ----------- Places -----------
            var places = new List<Place>
            {
                new Place { Id = 1,  City = "Eindhoven", District = "Centrum",       CentreGPS = "51.4416,5.4697" },
                new Place { Id = 2,  City = "Eindhoven", District = "Strijp",        CentreGPS = "51.4480,5.4485" },
                new Place { Id = 3,  City = "Eindhoven", District = "Gestel",        CentreGPS = "51.4147,5.4688" },
                new Place { Id = 4,  City = "Eindhoven", District = "Stratum",       CentreGPS = "51.4220,5.4938" },
                new Place { Id = 5,  City = "Eindhoven", District = "Tongelre",      CentreGPS = "51.4440,5.5075" },
                new Place { Id = 6,  City = "Eindhoven", District = "Woensel-Zuid",  CentreGPS = "51.4582,5.4779" },
                new Place { Id = 7,  City = "Eindhoven", District = "Woensel-Noord", CentreGPS = "51.4886,5.4672" },

                new Place { Id = 8,  City = "Veldhoven", District = "Centrum",            CentreGPS = "51.4186,5.4028" },
                new Place { Id = 9,  City = "Best",      District = null,            CentreGPS = "51.5075,5.3953" },
                new Place { Id = 10, City = "Son en Breugel", District = null,       CentreGPS = "51.5096,5.4904" },
                new Place { Id = 11, City = "Waalre",    District = null,            CentreGPS = "51.3915,5.4590" },
                new Place { Id = 12, City = "Geldrop",   District = null,            CentreGPS = "51.4215,5.5590" },
                new Place { Id = 13, City = "Nuenen",    District = null,            CentreGPS = "51.4750,5.5480" },
                new Place { Id = 14, City = "Helmond",   District = null,            CentreGPS = "51.4792,5.6570" },
                new Place { Id = 15, City = "Mierlo",    District = null,            CentreGPS = "51.4439,5.6204" },
                new Place { Id = 16, City = "Oirschot",  District = null,            CentreGPS = "51.5052,5.3137" },
                new Place { Id = 17, City = "Heeze",     District = null,            CentreGPS = "51.3831,5.5728" },
                new Place { Id = 18, City = "Leende",    District = null,            CentreGPS = "51.3502,5.5492" },
                new Place { Id = 19, City = "Maarheeze", District = null,            CentreGPS = "51.3141,5.6284" },
                new Place { Id = 20, City = "Soerendonk",District = null,            CentreGPS = "51.3012,5.6028" },

                new Place { Id = 21, City = "Vessem",    District = null,            CentreGPS = "51.4310,5.2886" },
                new Place { Id = 22, City = "Knegsel",   District = null,            CentreGPS = "51.4099,5.3340" },
                new Place { Id = 23, City = "Wintelre",  District = null,            CentreGPS = "51.4306,5.3822" },
                new Place { Id = 24, City = "Riethoven", District = null,            CentreGPS = "51.3503,5.3818" },
                new Place { Id = 25, City = "Steensel",  District = null,            CentreGPS = "51.3846,5.3704" },
                new Place { Id = 26, City = "Westerhoven", District = null,          CentreGPS = "51.3340,5.4138" },

                new Place { Id = 27, City = "Spoordonk", District = null,            CentreGPS = "51.5106,5.3501" },
                new Place { Id = 28, City = "Breugel",   District = null,            CentreGPS = "51.5157,5.4813" },
                new Place { Id = 29, City = "Aarle",     District = null,            CentreGPS = "51.5033,5.4132" },

                new Place { Id = 30, City = "Zesgehuchten", District = null,          CentreGPS = "51.4278,5.5322" },
                new Place { Id = 31, City = "Eeneind",   District = null,            CentreGPS = "51.4624,5.5622" },

                new Place { Id = 32, City = "Hapert",    District = null,            CentreGPS = "51.3646,5.2577" },
                new Place { Id = 33, City = "Hoogeloon", District = null,            CentreGPS = "51.3800,5.2560" },
                new Place { Id = 34, City = "Bladel",    District = null,            CentreGPS = "51.3686,5.2196" },
                new Place { Id = 35, City = "Casteren",  District = null,            CentreGPS = "51.4094,5.2338" },

                new Place { Id = 36, City = "Geldrop-Mierlo (area)", District = null,  CentreGPS = "51.4300,5.6000" },
                new Place { Id = 37, City = "Nuenen Gerwen (area)", District = null,   CentreGPS = "51.4850,5.5400" },

                new Place { Id = 38, City = "Oerle",     District = null,            CentreGPS = "51.4254,5.3648" },
                new Place { Id = 39, City = "Veldhoven", District = "Heikant",    CentreGPS = "51.4203,5.4155" },
                new Place { Id = 40, City = "Aalst", District = null,        CentreGPS = "51.3967,5.4642" },

            };
            await context.Places.AddRangeAsync(places);
            await context.SaveChangesAsync();

            // ----------- Users -----------
            var users = new List<User>
            {
                new User { Id = 1, Username = "Alice", Email = "alice@test.com", Phonenumber = "0600000000", PasswordHash = "Btd5kOga0bCQboFgEC27wQXmHO/7+ycka95ivGi4EXXAEOj303ehnFqmaGr3+rHi", CreatedAt = DateTime.Now },
                new User { Id = 2, Username = "Bob", Email = "bob@test.com", Phonenumber = "0611111111", PasswordHash = "DbjdjPrHA2CdSDtuDrpWqWbAcxPQIoxHxNz73a0P8CFWd/Sg55yo/+FTDbdsxtdL", CreatedAt = DateTime.Now },
                new User { Id = 3, Username = "Charlie", Email = "charlie@test.com", Phonenumber = "0622222222", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.Now },
                new User { Id = 4, Username = "Joseph", Email = "Joseph@test.com", Phonenumber = "0633333333", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.Now },
                new User { Id = 5, Username = "Diana", Email = "Diana@test.com", Phonenumber = "0644444444", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.Now }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // ----------- Journeys -----------
            var journeys = new List<Journey>
            {
                new Journey { Id = 1, StartId = 1, EndId = 16, CreatedAt = DateTime.Now, FinishedAt = null },
                new Journey { Id = 2, StartId = 2, EndId = 7, CreatedAt = DateTime.Now, FinishedAt = null },
                new Journey { Id = 3, StartId = 12, EndId = 10, CreatedAt = DateTime.Now.Subtract(TimeSpan.FromDays(1)), FinishedAt = null },
                new Journey { Id = 4, StartId = 20, EndId = 32, CreatedAt = DateTime.Now.Subtract(TimeSpan.FromDays(1)), FinishedAt = DateTime.Now }
            };
            await context.Journeys.AddRangeAsync(journeys);
            await context.SaveChangesAsync();

            // ----------- JourneyParticipants -----------
            var participants = new List<JourneyParticipant>
            {
                new JourneyParticipant { UserId = 1, JourneyId = 1, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now },
                new JourneyParticipant { UserId = 2, JourneyId = 1, Role = JourneyRole.Participant, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now },
                new JourneyParticipant { UserId = 3, JourneyId = 1, Role = JourneyRole.Participant, Status = RequestStatus.Pending, JoinedAt = DateTime.Now },
                new JourneyParticipant { UserId = 4, JourneyId = 1, Role = JourneyRole.Participant, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now },

                new JourneyParticipant { UserId = 2, JourneyId = 2, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now },
                new JourneyParticipant { UserId = 1, JourneyId = 2, Role = JourneyRole.Participant, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now },
                new JourneyParticipant { UserId = 4, JourneyId = 2, Role = JourneyRole.Participant, Status = RequestStatus.Pending, JoinedAt = DateTime.Now },
                new JourneyParticipant { UserId = 5, JourneyId = 2, Role = JourneyRole.Participant, Status = RequestStatus.Rejected, JoinedAt = DateTime.Now },

                new JourneyParticipant { UserId = 4, JourneyId = 3, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now.Subtract(TimeSpan.FromDays(12)) },
                new JourneyParticipant { UserId = 4, JourneyId = 4, Role = JourneyRole.Owner, Status = RequestStatus.Accepted, JoinedAt = DateTime.Now.Subtract(TimeSpan.FromDays(1)) }
            };
            await context.JourneyParticipants.AddRangeAsync(participants);
            await context.SaveChangesAsync();

            // ----------- JourneyMessages -----------
            var messages = new List<JourneyMessage>
            {
                new JourneyMessage { Id = 1, JourneyId = 1, SenderId = 1, Content = "Hi Bob!", SentAt = DateTime.Now },
                new JourneyMessage { Id = 2, JourneyId = 1, SenderId = 2, Content = "Hey Alice!", SentAt = DateTime.Now },
                new JourneyMessage { Id = 3, JourneyId = 1, SenderId = 2, Content = "Hello Charlie!", SentAt = DateTime.Now },

                new JourneyMessage { Id = 4, JourneyId = 2, SenderId = 1, Content = "Hi Bob!", SentAt = DateTime.Now },
                new JourneyMessage { Id = 5, JourneyId = 2, SenderId = 2, Content = "Hey Alice!", SentAt = DateTime.Now },
                new JourneyMessage { Id = 6, JourneyId = 2, SenderId = 2, Content = "Hello Charlie!", SentAt = DateTime.Now }
            };
            await context.Messages.AddRangeAsync(messages);
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
