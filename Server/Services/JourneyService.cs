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
        public async Task<ServiceResult<List<JourneyDto>>> GetJourneysByUserAsync(int userId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            List<Journey> journeys = await db.Journeys
                .Include(j => j.Participants)
                    .ThenInclude(p => p.User)
                .Where(j => j.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

            if (!journeys.Any())
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.ResourceNotFound, "No journeys found for this user");

            List<JourneyDto> dtos = journeys.Select(j => new JourneyDto
            {
                Id = j.Id,
                OwnerId = j.Participants.First(p => p.Role == JourneyRole.Owner).UserId,
                OwnerName = j.Participants.First(p => p.Role == JourneyRole.Owner).User.Username,
                StartGPS = j.StartGPS,
                EndGPS = j.EndGPS,
                CreatedAt = j.CreatedAt,
                FinishedAt = j.FinishedAt,
                IsOwner = (j.Participants.First(p => p.UserId == userId).Role == JourneyRole.Owner),
                IsParticipant = (j.Participants.Any(p => p.UserId == userId)),
                CanJoin = false
            }).ToList();
            return ServiceResult<List<JourneyDto>>.Succes(dtos);
        }

        /// <summary>
        /// Returns all participants of a given journey, including their role (Owner, Participant, etc.).
        /// </summary>
        public async Task<ServiceResult<List<JourneyParticipantDto>>> GetJourneyParticipantsAsync(int journeyId, int userId)
        {
            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                    .ThenInclude(jp => jp.User)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
                return ServiceResult<List<JourneyParticipantDto>>.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            if (journey.Participants == null || journey.Participants.Count == 0)
                return ServiceResult<List<JourneyParticipantDto>>.Fail(ServiceResultStatus.ResourceNotFound, "No participants found for this journey");

            if (!journey.Participants.Any(p => p.UserId == userId))
                return ServiceResult<List<JourneyParticipantDto>>.Fail(ServiceResultStatus.Unauthorized, "Access denied: You are not part of this journey");


            List<JourneyParticipantDto> participants = journey.Participants.Select(jp => new JourneyParticipantDto
            {
                UserId = jp.UserId,
                UserName = jp.User?.Username ?? "Unknown",
                Role = jp.Role,
                JoinedAt = jp.JoinedAt
            }).ToList();

            return ServiceResult<List<JourneyParticipantDto>>.Succes(participants);
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
                    .ThenInclude(p => p.User)
                .Where(j =>
                    j.Participants.Any(p => p.Role == JourneyRole.Owner && buddyIds.Contains(p.UserId)) &&
                    j.FinishedAt == null &&
                    !j.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

            List<JourneyDto> dtos = journeys.Select(j =>
            {
                JourneyParticipants? owner = j.Participants.FirstOrDefault(p => p.Role == JourneyRole.Owner);
                return new JourneyDto
                {
                    Id = j.Id,
                    OwnerId = owner?.UserId ?? 0,
                    OwnerName = owner?.User.Username ?? "Unknown",
                    StartGPS = j.StartGPS,
                    EndGPS = j.EndGPS,
                    CreatedAt = j.CreatedAt,
                    FinishedAt = j.FinishedAt,
                    IsOwner = false,
                    IsParticipant = false,
                    CanJoin = true
                };
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
                StartGPS = startGPS,
                EndGPS = endGPS,
                CreatedAt = DateTime.UtcNow,
                FinishedAt = null
            };

            journey.Participants.Add(new JourneyParticipants
            {
                UserId = user.Id,
                Role = JourneyRole.Owner,
                JoinedAt = DateTime.UtcNow
            });

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
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            if (journey.FinishedAt != null && journey.FinishedAt != DateTime.MinValue)
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Cannot join a finished journey");

            if (journey.Participants.Any(p => p.UserId == userId))
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Already part of this journey");

            // Find owner
            JourneyParticipants? owner = journey.Participants.FirstOrDefault(p => p.Role == JourneyRole.Owner);
            if (owner == null)
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Journey has no owner");


            ServiceResult<List<BuddyDto>> buddiesResult = await buddyService.GetBuddies(userId);
            if (buddiesResult.Status != ServiceResultStatus.Success || buddiesResult.Data == null)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Cannot retrieve buddies");

            List<int> buddyIds = buddiesResult.Data
                .Select(b => b.RequesterId == userId ? b.AddresseeId : b.RequesterId)
                .ToList();

            if (!buddyIds.Contains(owner.UserId))
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "You can only join journeys of your buddies");

            JourneyParticipants participant = new JourneyParticipants
            {
                UserId = userId,
                JourneyId = journey.Id,
                Role = JourneyRole.Participant,
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

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            JourneyParticipants? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || participant.Role != JourneyRole.Owner)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Access denied");

            if (journey.FinishedAt != DateTime.MinValue && journey.FinishedAt != null)
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Cannot update a finished journey");

            if (!string.IsNullOrWhiteSpace(startGps)) 
                journey.StartGPS = startGps;
            if (!string.IsNullOrWhiteSpace(endGps)) 
                journey.EndGPS = endGps;

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

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
            if (journey == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");

            JourneyParticipants? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || participant.Role != JourneyRole.Owner)
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Only the owner can finish the journey");

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

            JourneyParticipants? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "You are not a participant of this journey");

            // logic for owner leaving the journey
            if (participant.Role == JourneyRole.Owner)
            {
                if (journey.Participants.Count == 0)
                {
                    db.Journeys.Remove(journey);
                    await db.SaveChangesAsync();
                    return ServiceResult.Succes("Journey deleted as there were no participants.");
                }
                else
                {
                    JourneyParticipants? newOwner = journey.Participants.First(p => p.UserId != userId);
                    newOwner.Role = JourneyRole.Owner;

                    db.JourneyParticipants.Remove(participant);
                    await db.SaveChangesAsync();

                    return ServiceResult.Succes($"Ownership transferred to User {newOwner.UserId}. You have left the journey.");
                }
            }

            db.JourneyParticipants.Remove(participant);
            await db.SaveChangesAsync();

            return ServiceResult.Succes("Successfully left the journey");
        }
    }
}
