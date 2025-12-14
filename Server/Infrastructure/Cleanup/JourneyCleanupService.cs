namespace Server.Infrastructure.Cleanup
{
    using Microsoft.EntityFrameworkCore;
    using Server.Features.Chats;
    using Server.Features.Journeys;
    using Server.Infrastructure.Data;

    public class JourneyCleanupService
    {
        private readonly AppDbContext db;
        private readonly ILogger<JourneyCleanupService> logger;

        public JourneyCleanupService(AppDbContext db, ILogger<JourneyCleanupService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// Deletes sensitive journey-linked data for journeys finished more than 7 days ago:
        /// - JourneyMessage (chat history)
        /// - JourneyParticipant (join table)
        /// - Rating (user reference is nulled, but rating row is kept)
        /// Keeps the Journey row.
        /// </summary>
        public async Task CleanupOldJourneysAsync(CancellationToken ct)
        {
            DateTime cutoff = DateTime.UtcNow.AddDays(-7);

            // Only clean up journeys that are one week old.
            var journeyIds = await db.Journeys
                .Where(j => j.StartAt < cutoff)
                .Select(j => j.Id)
                .ToListAsync(ct);

            if (journeyIds.Count == 0)
                return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);

            try
            {
                // 1) Delete chat history
                var messages = db.Set<JourneyMessage>().Where(m => journeyIds.Contains(m.JourneyId));
                db.RemoveRange(messages);

                // 2) Delete participants (many-to-many join table data)
                var participants = db.Set<JourneyParticipant>().Where(p => journeyIds.Contains(p.JourneyId));
                db.RemoveRange(participants);

                // 3. Anonymize ratings (keep, but remove user reference)
                var ratings = await db.Set<Rating>()
                    .Where(r => journeyIds.Contains(r.JourneyId) && r.UserId != null)
                    .ToListAsync(ct);

                foreach (var rating in ratings)
                {
                    rating.UserId = null;
                }

                var affected = await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                logger.LogInformation("Journey cleanup completed. Journeys: {JourneyCount}, total affected rows: {AffectedRows}, cutoff: {Cutoff}",journeyIds.Count,affected,cutoff);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                logger.LogError(ex, "Journey cleanup failed. Cutoff: {Cutoff}", cutoff);
                throw;
            }
        }
    }

}
