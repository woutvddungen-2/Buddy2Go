using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Shared.Models.Dtos;

namespace Server.Services
{
    public class JourneyService
    {
        private readonly AppDbContext db;

        public JourneyService(AppDbContext db)
        {
            this.db = db;
        }
        // This method retrieves journeys for a specific user.
        public async Task<List<JourneyDto>> GetJourneysByUserAsync(int userId)
        {
            List<Journey> journeys = await db.Journeys
                .Where(j => j.Owner.Id == userId)
                .ToListAsync();

            List<JourneyDto> journeyDtos = journeys.Select(journey => new JourneyDto
            {
                OwnedBy = journey.OwnedBy,
                StartGPS = journey.StartGPS,
                EndGPS = journey.EndGPS,
                CreatedAt = journey.CreatedAt,
                FinishedAt = journey.FinishedAt
            }).ToList();

            return journeyDtos;
        }
    }
}
