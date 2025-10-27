using Microsoft.EntityFrameworkCore;
using Server.Common;
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
        public async Task<ServiceResult<List<JourneyDto>>> GetJourneysByUserAsync(int userId)
        {
            User? user = await db.Users.FindAsync(userId);
            if (user == null)
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            List<Journey> journeys = await db.Journeys
                .Where(j => j.Owner.Id == userId)
                .ToListAsync();

            if (journeys.Count == 0)
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.ResourceNotFound, "No journeys found for this user");



            return ServiceResult<List<JourneyDto>>.Succes(journeys.Select(journey => new JourneyDto
            {
                Id = journey.Id,
                OwnedBy = journey.OwnedBy,
                StartGPS = journey.StartGPS,
                EndGPS = journey.EndGPS,
                CreatedAt = journey.CreatedAt,
                FinishedAt = journey.FinishedAt
            }).ToList());
        }


        // This method adds a new journey for a specific user.
        public async Task<ServiceResult> AddJourneyAsync(int userId, string startGPS, string endGPS)
        {
            User? user = await db.Users.FindAsync(userId);
            if (user == null)
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            if (startGPS == null || endGPS == null)
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "StartGPS and EndGPS cannot be null");

            Journey journey = new Journey
            {
                Owner = user,
                OwnedBy = user.Id,
                StartGPS = startGPS, // Default start GPS
                EndGPS = endGPS,   // Default end GPS
                CreatedAt = DateTime.UtcNow,
                FinishedAt = DateTime.MinValue // Not finished yet
            };

            await db.Journeys.AddAsync(journey);
            await db.SaveChangesAsync();

            return ServiceResult.Succes();
        }

        /// <summary>
        /// Updates the GPS coordinates of an existing journey.
        /// </summary>
        /// <param name="userId"> User Id of the owner, Cannot be Null or Empty</param>
        /// <param name="journeyId"> Journey Id to update, Cannot be Null or Empty</param>
        /// <param name="startGps"> New Start GPS coordinates, can be Null</param>
        /// <param name="endGps"> New End GPS coordinates, can be Null</param>
        /// <returns>Updated JourneyDto or null if not found</returns>
        /// <exception cref="ArgumentException"> Thrown when user or journey is not found or access is denied</exception>
        public async Task<ServiceResult> UpdateJourneyGpsAsync(int userId, int journeyId, string? startGps, string? endGps)
        {
            User? user = await db.Users.FindAsync(userId);
            if (user == null)
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            
            Journey? journey = await db.Journeys.FindAsync(journeyId);
            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            if (journey .FinishedAt != DateTime.MinValue)
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Cannot update a finished journey");

            if (journey.OwnedBy != userId)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Access denied");
            
            

            if (!string.IsNullOrWhiteSpace(startGps)) journey.StartGPS = startGps;
            if (!string.IsNullOrWhiteSpace(endGps)) journey.EndGPS = endGps;

            await db.SaveChangesAsync();

            return ServiceResult.Succes();
        }

        public async Task<ServiceResult> FinishJourneyAsync(int userId, int journeyId)
        {
            User? user = await db.Users.FindAsync(userId);
            if (user == null)
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");

            Journey? journey = await db.Journeys.FindAsync(journeyId);
            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            if (journey.OwnedBy != userId)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Access denied");

            journey.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return ServiceResult.Succes();

        }
    }
}
