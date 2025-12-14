namespace Server.Infrastructure.Cleanup
{
    using Microsoft.EntityFrameworkCore;
    using Server.Features.Chats;
    using Server.Features.DangerousPlaces;
    using Server.Features.Journeys;
    using Server.Infrastructure.Data;

    public class DangerousPlaceCleanupService
    {
        private readonly AppDbContext db;
        private readonly ILogger<DangerousPlaceCleanupService> logger;

        public DangerousPlaceCleanupService(AppDbContext db, ILogger<DangerousPlaceCleanupService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// Cleans up dangerous places reported more than 1 day ago by anonymizing the reporter:
        /// </summary>
        public async Task Cleanup(CancellationToken ct)
        {
            DateTime cutoff = DateTime.UtcNow.AddDays(-7);
            List<DangerousPlace> places = await db.Set<DangerousPlace>().Where(p => p.ReportedAt < cutoff && p.ReportedById != -1).ToListAsync(ct);

            if (places.Count == 0)
                return;

            foreach (var place in places)
            {
                place.ReportedById = -1;
            }

            await db.SaveChangesAsync(ct);
        }
    }

}
