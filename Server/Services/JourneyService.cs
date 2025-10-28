using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Data;
using Server.Models;
using Shared.Models;
using Shared.Models.Dtos;

namespace Server.Services
{


    public class JourneyService
    {
        private readonly AppDbContext db;
        private readonly BuddyService buddyService;

        public JourneyService(AppDbContext db, BuddyService buddyService)
        {
            this.db = db;
            this.buddyService = buddyService;
        }

        /// <summary>
        /// Retrieves all journeys associated with a specific user, either as an owner or participant.
        /// </summary>
        public async Task<ServiceResult<List<JourneyDto>>> GetJourneysByUserAsync(int userId, bool onlyOpen = false)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            var query = db.Journeys
                .Include(j => j.Participants)
                .Where(j =>
                    j.Owner!.Id == userId ||
                    j.Participants.Any(p => p.UserId == userId));

            if (onlyOpen)
            {
                query = query.Where(j => j.FinishedAt == null);
            }

            List<Journey> journeys = await query.ToListAsync();

            if (journeys.Count == 0)
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.ResourceNotFound, "No journeys found for this user");

            List<JourneyDto> dtos = journeys.Select(j => new JourneyDto
            {
                Id = j.Id,
                OwnedBy = j.OwnedBy,
                StartGPS = j.StartGPS,
                EndGPS = j.EndGPS,
                CreatedAt = j.CreatedAt,
                FinishedAt = j.FinishedAt,
                IsOwner = (j.OwnedBy == userId),
                IsParticipant = (j.Participants.Any(p => p.UserId == userId)),
                CanJoin = false
            }).ToList();
            return ServiceResult<List<JourneyDto>>.Succes(dtos);
        }

        /// <summary>
        /// Retrieves all open journeys created by the user's buddies that the user is not already part of.
        /// </summary>
        public async Task<ServiceResult<List<JourneyDto>>> GetBuddyJourneysAsync(int userId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            List<int> buddyIds = await db.Buddys
                .Where(b => (b.RequesterId == userId || b.AddresseeId == userId) && b.Status == RequestStatus.Accepted)
                .Select(b => b.RequesterId == userId ? b.AddresseeId : b.RequesterId)
                .ToListAsync();

            List<Journey> journeys = await db.Journeys
                .Include(j => j.Participants)
                .Where(j =>
                    buddyIds.Contains(j.OwnedBy) &&
                    j.FinishedAt == null &&
                    !j.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

            List<JourneyDto> dtos = journeys.Select(j => new JourneyDto
            {
                Id = j.Id,
                OwnedBy = j.OwnedBy,
                StartGPS = j.StartGPS,
                EndGPS = j.EndGPS,
                CreatedAt = j.CreatedAt,
                FinishedAt = j.FinishedAt,
                IsOwner = false,
                IsParticipant = false,
                CanJoin = true
            }).ToList();

            return ServiceResult<List<JourneyDto>>.Succes(dtos);
        }

        /// <summary>
        /// Creates a new journey for a user.
        /// </summary>
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
                StartGPS = startGPS,
                EndGPS = endGPS,
                CreatedAt = DateTime.UtcNow,
                FinishedAt = null
            };

            await db.Journeys.AddAsync(journey);
            await db.SaveChangesAsync();

            return ServiceResult.Succes("Succesfully added Journey");
        }

        /// <summary>
        /// Allows a user to join an existing journey if they are buddies with the journey owner.
        /// </summary>
        public async Task<ServiceResult> JoinJourneyAsync(int userId, int journeyId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .Include(j => j.Owner)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            if (journey.FinishedAt != null && journey.FinishedAt != DateTime.MinValue)
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Cannot join a finished journey");

            if (journey.OwnedBy == userId || journey.Participants.Any(p => p.UserId == userId))
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Already part of this journey");

            ServiceResult<List<BuddyDto>> buddiesResult = await buddyService.GetBuddies(userId);
            if (buddiesResult.Status != ServiceResultStatus.Success || buddiesResult.Data == null)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Cannot retrieve buddies");

            List<int> buddyIds = buddiesResult.Data
                .Select(b => b.RequesterId == userId ? b.AddresseeId : b.RequesterId)
                .ToList();

            if (!buddyIds.Contains(journey.OwnedBy))
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "You can only join journeys of your buddies");

            JourneyParticipants participant = new JourneyParticipants
            {
                JourneyId = journeyId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            db.JourneyParticipants.Add(participant);
            await db.SaveChangesAsync();

            return ServiceResult.Succes("Successfully joined the journey");
        }


        /// <summary>
        /// Updates the GPS coordinates of an existing journey.
        /// </summary>
        public async Task<ServiceResult> UpdateJourneyGpsAsync(int userId, int journeyId, string? startGps, string? endGps)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
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

        /// <summary>
        /// Marks a journey as finished.
        /// </summary>
        public async Task<ServiceResult> FinishJourneyAsync(int userId, int journeyId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");

            Journey? journey = await db.Journeys.FindAsync(journeyId);
            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            if (journey.OwnedBy != userId)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Access denied");

            if (journey.FinishedAt != null)
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Journey is already finished");

            journey.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return ServiceResult.Succes();

        }

        /// <summary>
        /// Allows a user to leave a journey they are participating in.
        /// </summary>
        public async Task<ServiceResult> LeaveJourneyAsync(int userId, int journeyId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            // logic for owner leaving the journey
            if (journey.OwnedBy == userId)
            {
                if (journey.Participants.Any())
                {
                    // Transfer ownership to first participant
                    JourneyParticipants newOwner = journey.Participants.First();
                    journey.OwnedBy = newOwner.UserId;
                    journey.Owner = await db.Users.FindAsync(newOwner.UserId);

                    // Remove the new owner from participants since they are now the owner
                    db.JourneyParticipants.Remove(newOwner);

                    await db.SaveChangesAsync();
                    return ServiceResult.Succes($"Ownership transferred to User {newOwner.UserId}. You have left the journey.");
                }
                else
                {
                    // No participants left, delete the journey
                    db.Journeys.Remove(journey);
                    await db.SaveChangesAsync();
                    return ServiceResult.Succes("Journey deleted as there were no participants.");
                }
            }

            // logic for participant leaving the journey
            JourneyParticipants? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "You are not a participant of this journey");

            db.JourneyParticipants.Remove(participant);
            await db.SaveChangesAsync();

            return ServiceResult.Succes("Successfully left the journey");
        }
    }
}
