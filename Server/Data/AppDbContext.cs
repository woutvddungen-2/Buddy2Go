using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Journey> Journeys { get; set; }
        public DbSet<JourneyMessage> JourneyMessages { get; set; }
        public DbSet<JourneyParticipant> JourneyParticipants { get; set; }
        public DbSet<DangerousPlace> DangerousPlaces { get; set; }
        public DbSet<Buddy> Buddys { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Rating> Ratings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --------------------
            // Configure composite keys
            // --------------------
            modelBuilder.Entity<JourneyParticipant>()
                .HasKey(jp => new { jp.UserId, jp.JourneyId });
            
            modelBuilder.Entity<Buddy>()
                .HasKey(b => new { b.RequesterId, b.AddresseeId });

            modelBuilder.Entity<Rating>()
                .HasKey(r => new { r.JourneyId, r.UserId});

            // --------------------
            // Configure relationships
            // --------------------
            modelBuilder.Entity<JourneyParticipant>()
                .HasOne(jp => jp.User)
                .WithMany(u => u.JourneyParticipations)
                .HasForeignKey(jp => jp.UserId);
            modelBuilder.Entity<JourneyParticipant>()
                .HasOne(jp => jp.Journey)
                .WithMany(j => j.Participants)
                .HasForeignKey(jp => jp.JourneyId);

            modelBuilder.Entity<JourneyMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId);
            modelBuilder.Entity<JourneyMessage>()
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

            modelBuilder.Entity<Journey>()
                .HasOne(j => j.Start)
                .WithMany()
                .HasForeignKey(j => j.StartId);
            modelBuilder.Entity<Journey>()
                .HasOne(j => j.End)
                .WithMany()
                .HasForeignKey(j => j.EndId);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Journey)
                .WithMany()
                .HasForeignKey(r => r.JourneyId);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);



            modelBuilder.Entity<Place>().HasData(
                new Place { Id = 1,  City = "Eindhoven", District = "Centrum",       CentreGPS = "51.4416,5.4697" },
                new Place { Id = 2,  City = "Eindhoven", District = "Strijp",        CentreGPS = "51.4480,5.4485" },
                new Place { Id = 3,  City = "Eindhoven", District = "Gestel",        CentreGPS = "51.4147,5.4688" },
                new Place { Id = 4,  City = "Eindhoven", District = "Stratum",       CentreGPS = "51.4220,5.4938" },
                new Place { Id = 5,  City = "Eindhoven", District = "Tongelre",      CentreGPS = "51.4440,5.5075" },
                new Place { Id = 6,  City = "Eindhoven", District = "Woensel-Zuid",  CentreGPS = "51.4582,5.4779" },
                new Place { Id = 7,  City = "Eindhoven", District = "Woensel-Noord", CentreGPS = "51.4886,5.4672" },

                new Place { Id = 8,  City = "Veldhoven", District = "Centrum",       CentreGPS = "51.4186,5.4028" },
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

                new Place { Id = 30, City = "Zesgehuchten", District = null,         CentreGPS = "51.4278,5.5322" },
                new Place { Id = 31, City = "Eeneind",   District = null,            CentreGPS = "51.4624,5.5622" },

                new Place { Id = 32, City = "Hapert",    District = null,            CentreGPS = "51.3646,5.2577" },
                new Place { Id = 33, City = "Hoogeloon", District = null,            CentreGPS = "51.3800,5.2560" },
                new Place { Id = 34, City = "Bladel",    District = null,            CentreGPS = "51.3686,5.2196" },
                new Place { Id = 35, City = "Casteren",  District = null,            CentreGPS = "51.4094,5.2338" },

                new Place { Id = 36, City = "Gerwen",    District = null,               CentreGPS = "51.4850,5.5400" },
                new Place { Id = 37, City = "Oerle",     District = null,            CentreGPS = "51.4254,5.3648" },
                new Place { Id = 38, City = "Veldhoven", District = "Heikant",       CentreGPS = "51.4203,5.4155" },
                new Place { Id = 39, City = "Aalst",     District = null,                CentreGPS = "51.3967,5.4642" }
           );
        }

    }
}
