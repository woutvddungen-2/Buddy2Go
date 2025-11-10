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
