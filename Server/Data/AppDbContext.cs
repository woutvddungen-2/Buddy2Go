using Microsoft.EntityFrameworkCore
;
using Server.Models;
using Shared.Models;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Journey> Journeys { get; set; }
        public DbSet<JourneyMessages> Messages { get; set; }
        public DbSet<JourneyParticipants> JourneyParticipants { get; set; }
        public DbSet<DangerousPlace> DangerousPlace { get; set; }
        public DbSet<Buddy> Buddys { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --------------------
            // Seed Users
            // --------------------
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "Alice", Email = "alice@test.com", Phonenumber = "0612345678", PasswordHash = "Btd5kOga0bCQboFgEC27wQXmHO/7+ycka95ivGi4EXXAEOj303ehnFqmaGr3+rHi", CreatedAt = DateTime.UtcNow },
                new User { Id = 2, Username = "Bob", Email = "bob@test.com", Phonenumber = "0687654321", PasswordHash = "DbjdjPrHA2CdSDtuDrpWqWbAcxPQIoxHxNz73a0P8CFWd/Sg55yo/+FTDbdsxtdL", CreatedAt = DateTime.UtcNow },
                new User { Id = 3, Username = "Charlie", Email = "charlie@test.com", Phonenumber = "0678901234", PasswordHash = "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", CreatedAt = DateTime.UtcNow }
            );

            // --------------------
            // Seed Journey
            // --------------------
            modelBuilder.Entity<Journey>().HasData(
                new Journey { Id = 1, OwnedBy = 1, StartGPS = "52.370216,4.895168", EndGPS = "51.924420,4.477733", CreatedAt = DateTime.UtcNow },
                new Journey { Id = 2, OwnedBy = 2, StartGPS = "52.090737,5.121420", EndGPS = "51.441642,5.469722", CreatedAt = DateTime.UtcNow }
            );

            // --------------------
            // Seed JourneyParticipants (composite key)
            // --------------------
            modelBuilder.Entity<JourneyParticipants>().HasData(
                new JourneyParticipants { UserId = 1, JourneyId = 1, JoinedAt = DateTime.UtcNow },
                new JourneyParticipants { UserId = 2, JourneyId = 1, JoinedAt = DateTime.UtcNow },
                new JourneyParticipants { UserId = 2, JourneyId = 2, JoinedAt = DateTime.UtcNow },
                new JourneyParticipants { UserId = 3, JourneyId = 2, JoinedAt = DateTime.UtcNow }
            );

            // --------------------
            // Seed JourneyMessages
            // --------------------
            modelBuilder.Entity<JourneyMessages>().HasData(
                new JourneyMessages { Id = 1, JourneyId = 1, SenderId = 1, Content = "Hi Bob!", SentAt = DateTime.UtcNow },
                new JourneyMessages { Id = 2, JourneyId = 1, SenderId = 2, Content = "Hey Alice!", SentAt = DateTime.UtcNow },
                new JourneyMessages { Id = 3, JourneyId = 2, SenderId = 2, Content = "Hello Charlie!", SentAt = DateTime.UtcNow }
            );

            // --------------------
            // Seed DangerousPlace
            // --------------------
            modelBuilder.Entity<DangerousPlace>().HasData(
                new DangerousPlace { Id = 1, ReportedById = 1, GPS = "52.370216,4.895168", PlaceType = DangerousPlaceType.PoorLighting, Description = "Very dark street, watch out!" },
                new DangerousPlace { Id = 2, ReportedById = 2, GPS = "51.924420,4.477733", PlaceType = DangerousPlaceType.HazardousRoad, Description = "Lots of garbage here" }
            );

            //---------------------
            // Seed Buddy (composite key)
            //---------------------
            modelBuilder.Entity<Buddy>().HasData(
                new Buddy { RequesterId = 1, AddresseeId = 2, Status = RequestStatus.Accepted },
                new Buddy { RequesterId = 2, AddresseeId = 3, Status = RequestStatus.Pending }
            );

            // --------------------
            // Configure composite key for JourneyParticipants
            // --------------------
            modelBuilder.Entity<JourneyParticipants>()
                .HasKey(jp => new { jp.UserId, jp.JourneyId });
            modelBuilder.Entity<Buddy>()
                .HasKey(b => new { b.RequesterId, b.AddresseeId });

            // --------------------
            // Configure relationships
            // --------------------
            modelBuilder.Entity<JourneyParticipants>()
                .HasOne(jp => jp.User)
                .WithMany(u => u.JourneyParticipations)
                .HasForeignKey(jp => jp.UserId);

            modelBuilder.Entity<JourneyParticipants>()
                .HasOne(jp => jp.Journey)
                .WithMany(j => j.Participants)
                .HasForeignKey(jp => jp.JourneyId);

            modelBuilder.Entity<JourneyMessages>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<JourneyMessages>()
                .HasOne(m => m.Journey)
                .WithMany(j => j.Messages)
                .HasForeignKey(m => m.JourneyId);

            modelBuilder.Entity<DangerousPlace>()
                .HasOne(d => d.ReportedBy)
                .WithMany(u => u.Reports)
                .HasForeignKey(d => d.ReportedById);

            modelBuilder.Entity<Journey>()
                .HasOne(j => j.Owner)
                .WithMany(u => u.OwnedJourneys)
                .HasForeignKey(j => j.OwnedBy);

            modelBuilder.Entity<Buddy>()
                .HasOne(b => b.Requester)
                .WithMany()
                .HasForeignKey(b => b.RequesterId);

            modelBuilder.Entity<Buddy>()
                .HasOne(b => b.Addressee)
                .WithMany()
                .HasForeignKey(b => b.AddresseeId);
        }

    }
}
