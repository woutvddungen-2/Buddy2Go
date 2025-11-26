using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Features.Buddies;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Shared.Models.Dtos;
using Shared.Models.enums;

namespace Server.Features.Journeys
{


    public class JourneyService
    {
        private readonly AppDbContext db;
        private readonly BuddyService buddyService;
        private readonly ILogger logger;

        public JourneyService(AppDbContext db, BuddyService buddyService, ILogger<JourneyService> logger)
        {
            this.db = db;
            this.buddyService = buddyService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all journeys associated with a specific user, either as an owner or participant.
        /// </summary>
        public async Task<ServiceResult<List<JourneyDto>>> GetJourneysByUserAsync(int userId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("GetMyJourneys Failed, User: {user} not found", userId);
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.UserNotFound, $"User not found in database");
            }

            List<Journey> journeys = await db.Journeys
                .Include(j => j.Start)
                .Include(j => j.End)
                .Include(j => j.Participants)
                    .ThenInclude(p => p.User)
                .Where(j => j.Participants.Any(p => p.UserId == userId &&
                            (p.Status == RequestStatus.Accepted || p.Role == JourneyRole.Owner)))
                .ToListAsync();

            List<JourneyDto> dtos = journeys.Select(j => new JourneyDto
            {
                Id = j.Id,
                OwnerId = j.Participants.First(p => p.Role == JourneyRole.Owner).UserId,
                OwnerName = j.Participants.First(p => p.Role == JourneyRole.Owner).User.Username,
                Start = new PlaceDto { Id = j.Start.Id, City = j.Start.City, District = j.Start.District, CentreGPS = j.Start.CentreGPS },
                End = new PlaceDto { Id = j.End.Id, City = j.End.City, District = j.End.District, CentreGPS = j.End.CentreGPS },
                CreatedAt = j.CreatedAt,
                StartAt = j.StartAt,
                FinishedAt = j.FinishedAt,
                Participants = j.Participants
                    .Select(p => new JourneyParticipantDto
                    {
                        UserId = p.UserId,
                        UserName = p.User?.Username ?? "Unknown",
                        Status = p.Status,
                        Role = p.Role,
                        JoinedAt = p.JoinedAt
                    })
                    .ToList()
            }).ToList();

            logger.LogDebug("GetMyJourneys, successfully retrieved {count} journey(s) for User:{user}", dtos.Count, userId);
            return ServiceResult<List<JourneyDto>>.Succes(dtos);
        }

        /// <summary>
        /// Retrieves all open journeys created by the user's buddies that the user is not already part of.
        /// </summary>
        public async Task<ServiceResult<List<JourneyDto>>> GetBuddyJourneysAsync(int userId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("GetBuddyJourneys Failed, User: {user} not found", userId);
                return ServiceResult<List<JourneyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            List<int> buddyIds = await db.Buddys
                .Where(b => (b.RequesterId == userId || b.AddresseeId == userId) && b.Status == RequestStatus.Accepted)
                .Select(b => b.RequesterId == userId ? b.AddresseeId : b.RequesterId)
                .ToListAsync();

            if (buddyIds.Count < 1)
            {
                logger.LogDebug("GetBuddyJourneys, no buddies for User:{user}", userId);
                return ServiceResult<List<JourneyDto>>.Succes(new List<JourneyDto>());
            }

            List<Journey> journeys = await db.Journeys
                .Include(j => j.Start)
                .Include(j => j.End)
                .Include(j => j.Participants)
                    .ThenInclude(p => p.User)
                .Where(j =>
                    j.Participants.Any(p => p.Role == JourneyRole.Owner && buddyIds.Contains(p.UserId)) &&
                    j.FinishedAt == null &&
                    !j.Participants.Any(p =>
                    p.UserId == userId && (p.Status == RequestStatus.Accepted || p.Status == RequestStatus.Blocked))
                )
                .ToListAsync();

            List<JourneyDto> dtos = journeys.Select(j => new JourneyDto
            {
                Id = j.Id,
                OwnerId = j.Participants.First(p => p.Role == JourneyRole.Owner).UserId,
                OwnerName = j.Participants.First(p => p.Role == JourneyRole.Owner).User.Username,
                Start = new PlaceDto { Id = j.Start.Id, City = j.Start.City, District = j.Start.District, CentreGPS = j.Start.CentreGPS },
                End = new PlaceDto { Id = j.End.Id, City = j.End.City, District = j.End.District, CentreGPS = j.End.CentreGPS },
                CreatedAt = j.CreatedAt,
                StartAt = j.StartAt,
                FinishedAt = j.FinishedAt,
                Participants = j.Participants
                .Select(p => new JourneyParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User?.Username ?? "Unknown",
                    Status = p.Status,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt
                })
                .ToList()
            }).ToList();

            logger.LogDebug("GetBuddyJourneys, successfully retrieved {count} journey(s) for User:{user}", dtos.Count, userId);
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
            {
                logger.LogWarning("GetJourneyParticipants Failed, no Journey Found, User:{User} JourneyId:{journey}", userId, journeyId);
                return ServiceResult<List<JourneyParticipantDto>>.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }

            if (journey.Participants == null || journey.Participants.Count == 0)
            {
                logger.LogWarning("GetJourneyParticipants Failed, no Participants Found, User:{User} JourneyId:{journey}", userId, journeyId);
                return ServiceResult<List<JourneyParticipantDto>>.Fail(ServiceResultStatus.ResourceNotFound, "No participants found for this journey");
            }

            if (!journey.Participants.Any(p => p.UserId == userId))
            {
                logger.LogWarning("GetJourneyParticipants Failed, acced denied for User:{User} JourneyId:{journey}", userId, journeyId);
                return ServiceResult<List<JourneyParticipantDto>>.Fail(ServiceResultStatus.Unauthorized, "Access denied: You are not part of this journey");
            }

            List<JourneyParticipantDto> participants = journey.Participants.Select(jp => new JourneyParticipantDto
            {
                UserId = jp.UserId,
                UserName = jp.User?.Username ?? "Unknown",
                Role = jp.Role,
                Status = jp.Status,
                JoinedAt = jp.JoinedAt
            }).ToList();

            logger.LogDebug("GetJourneyParticipants,successfully retrieved {Count} Participants for User:{user}, journeyId:{journey}",participants.Count, userId, journeyId);
            return ServiceResult<List<JourneyParticipantDto>>.Succes(participants);
        }

        /// <summary>
        /// Creates a new journey for a user.
        /// </summary>
        public async Task<ServiceResult> AddJourneyAsync(int userId, int startPlaceId, int endPlaceId, DateTime startAt)
        {
            User? user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                logger.LogWarning("AddJourney, user not found, User:{user}", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }
            if (!await db.Places.AnyAsync(p => p.Id == startPlaceId) || !await db.Places.AnyAsync(p => p.Id == endPlaceId))
            {
                logger.LogWarning("AddJourney Failed for User {user}, startPlace or EndPlace cannot be found in database", userId);
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "startPlace or EndPlace cannot be found in database");
            }

            Journey journey = new Journey
            {
                StartId = startPlaceId,
                EndId = endPlaceId,
                StartAt = startAt,
                FinishedAt = null,
                Participants = [],
                Messages = []
            };

            journey.Participants.Add(new JourneyParticipant
            {
                UserId = user.Id,
                User = user,
                Journey = journey,
                Role = JourneyRole.Owner,
                Status = RequestStatus.Accepted,
                JoinedAt = DateTime.UtcNow
            });

            await db.Journeys.AddAsync(journey);

            await db.SaveChangesAsync();
            await db.Entry(journey).ReloadAsync();
            await db.Entry(journey).Collection(j => j.Participants).LoadAsync();

            logger.LogInformation("AddJourney, journey added successfully for User:{user}", userId);
            return ServiceResult.Succes("Succesfully added Journey");
        }

        /// <summary>
        /// Allows a user to join an existing journey if they are buddies with the journey owner.
        /// </summary>
        public async Task<ServiceResult> SendJoinJourneyRequest(int userId, int journeyId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("SendJoinJourneyRequest Failed, User: {user} not found", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
            {
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }
            if (journey.FinishedAt != null && journey.FinishedAt != DateTime.MinValue)
            {
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Cannot join a finished journey");
            }

            JourneyParticipant? existing = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (existing != null)
            {
                if (existing?.Status == RequestStatus.Rejected)
                {
                    existing.Status = RequestStatus.Pending;
                    await db.SaveChangesAsync();
                    return ServiceResult.Succes($"Changed Rejected from {userId} in journey {journeyId} to pending.");
                }
                else if (existing?.Status == RequestStatus.Pending)
                {
                    logger.LogWarning("SendJoinJourneyRequest Failed, Join request already pending for user: {user}, Journey {journey}", userId, journeyId);
                    return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Join request already pending");
                }
                else if (existing?.Status == RequestStatus.Accepted)
                {
                    logger.LogWarning("SendJoinJourneyRequest Failed, Join request already accepted for user: {user}, Journey {journey}", userId, journeyId);
                    return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Already part of this journey");
                }
                else if (existing?.Status == RequestStatus.Blocked)
                {
                    logger.LogWarning("SendJoinJourneyRequest Failed, Join request blocked for user: {user}, Journey {journey}", userId, journeyId);
                    return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Join request was blocked");
                }
            }

            JourneyParticipant? owner = journey.Participants.FirstOrDefault(p => p.Role == JourneyRole.Owner);
            if (owner == null)
            {
                logger.LogWarning("SendJoinJourneyRequest Failed, Journey {journey} has no owner", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Journey has no owner");
            }

            ServiceResult<List<BuddyDto>> buddiesResult = await buddyService.GetBuddies(userId);
            if (buddiesResult.Status != ServiceResultStatus.Success || buddiesResult.Data == null)
            {
                logger.LogWarning("SendJoinJourneyRequest Failed, Journey {journey} çannot retrieve buddies", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Cannot retrieve buddies");
            }

            List<int> buddyIds = buddiesResult.Data
                .Select(b => b.Requester.Id == userId ? b.Addressee.Id : b.Requester.Id)
                .ToList();

            if (!journey.Participants.Any(p => buddyIds.Contains(p.UserId)))
            {
                logger.LogWarning("SendJoinJourneyRequest Failed, User {user} cannot join Journey {journey} because there is no buddy connection",userId, journeyId);
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "You can only join journeys where a buddy is already participating");
            }


            JourneyParticipant participant = new JourneyParticipant
            {
                UserId = userId,
                JourneyId = journey.Id,
                Role = JourneyRole.Participant,
                Status = RequestStatus.Pending
            };

            db.JourneyParticipants.Add(participant);
            await db.SaveChangesAsync();

            logger.LogInformation("SendJoinJourneyRequest succesful for user {user} in journey {journey} ", userId, journeyId);
            return ServiceResult.Succes("Join request sent. Awaiting owner approval.");
        }

        /// <summary>
        /// Responds to a join request of another user
        /// </summary>
        public async Task<ServiceResult> RespondToJourneyRequest(int ownerId, int journeyId, int requesterId, RequestStatus status)
        {
            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
            {
                logger.LogWarning("RespondtoJourneyRequest failed, Journey {Journey} not found", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }


            JourneyParticipant? owner = journey.Participants.FirstOrDefault(p => p.UserId == ownerId && p.Role == JourneyRole.Owner);
            if (owner == null)
            {
                logger.LogWarning("RespondtoJourneyRequest failed, User {user} is not owner of Journey {Journey}", ownerId, journeyId);
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Only the owner can respond to join requests");
            }
            JourneyParticipant? request = journey.Participants.FirstOrDefault(p => p.UserId == requesterId);
            if (request == null)
            {
                logger.LogWarning("RespondtoJourneyRequest failed, in Journey {Journey}, Join request not found for user {userId}", journeyId, ownerId);
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Join request not found");
            }
            if (request.Status != RequestStatus.Pending)
            {
                logger.LogWarning("RespondtoJourneyRequest failed, in Journey {Journey}, Join request already handled for user {userId}", journeyId, ownerId);
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "This join request has already been handled");
            }

            if (status == RequestStatus.Accepted)
            {
                request.Status = RequestStatus.Accepted;
                request.JoinedAt = DateTime.UtcNow;
            }
            else
            {
                request.Status = status;
            }

            await db.SaveChangesAsync();
            if (status == RequestStatus.Accepted)
            {
                logger.LogInformation("Join request for Journey {journey} for owner {user} approved succesfully", journeyId, ownerId);
                return ServiceResult.Succes("Join request approved successfully");
            }

            logger.LogInformation("Join request for Journey {journey} for owner {user} rejected succesfully", journeyId, ownerId);
            return ServiceResult.Succes("Join request rejected successfully");
        }


        /// <summary>
        /// Updates the GPS coordinates of an existing journey.
        /// </summary>
        public async Task<ServiceResult> UpdateJourneyGpsAsync(int userId, int journeyId, int startPlaceId, int endPlaceId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("UpdateJourney, user not found, User:{user}", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }
            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
            if (journey == null)
            {
                logger.LogWarning("UpdateJourney, Journey not found, Journey:{journey}", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }
            JourneyParticipant? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || participant.Role != JourneyRole.Owner)
            {
                logger.LogWarning("UpdateJourney, User {user} is not owner of Journey:{journey}", userId, journeyId);
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Access denied");
            }
            if (journey.FinishedAt != DateTime.MinValue && journey.FinishedAt != null)
            {
                logger.LogWarning("UpdateJourney, Journey:{journey} already finished", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Cannot update a finished journey");
            }
            if (startPlaceId > 0)
            {
                if (!await db.Places.AnyAsync(p => p.Id == startPlaceId))
                {
                    logger.LogWarning("UpdateJourney Failed for User {user}, startPlace cannot be found in database", userId);
                    return ServiceResult.Fail(ServiceResultStatus.ValidationError, "startPlace cannot be found in database");
                }
            }
            if (endPlaceId > 0)
            {
                if (!await db.Places.AnyAsync(p => p.Id == endPlaceId))
                {
                    logger.LogWarning("UpdateJourney Failed for User {user}, EndPlace cannot be found in database", userId);
                    return ServiceResult.Fail(ServiceResultStatus.ValidationError, "EndPlace cannot be found in database");
                }
            }
            await db.SaveChangesAsync();

            logger.LogInformation("UpdateJourney, Journey {journey}, succesfully updated by user: {user}", journeyId, userId);
            return ServiceResult.Succes();
        }

        /// <summary>
        /// Marks a journey as finished.
        /// </summary>
        public async Task<ServiceResult> FinishJourneyAsync(int userId, int journeyId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("FinishJourney failed, User {user} not found", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
            if (journey == null)
            {
                logger.LogWarning("FinishJourney Failed, Journey {journey} not found", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }
            JourneyParticipant? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || participant.Role != JourneyRole.Owner)
            {
                logger.LogWarning("FinishJourney Failed, User {user} is not the owner of Journey {journey}", userId, journeyId);
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "Only the owner can finish the journey");
            }
            if (journey.FinishedAt != null)
            {
                logger.LogWarning("FinishJourney Failed, Journey {journey} already finished", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Journey is already finished");
            }
            journey.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            logger.LogInformation("FinishJourney, successfully finished journey for User:{user}, Journey:{journey}", userId, journeyId);
            return ServiceResult.Succes();

        }

        /// <summary>
        /// Allows a user to leave a journey they are participating in.
        /// </summary>
        public async Task<ServiceResult> LeaveJourneyAsync(int userId, int journeyId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("LeaveJourney failed, user not found, User:{user}", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);
            if (journey == null)
            {
                logger.LogWarning("LeaveJourney failed, Journey {journey} not found", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }
            JourneyParticipant? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                logger.LogWarning("LeaveJourney failed, user {user} not participant of Journey {journey}", userId, journeyId);
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "You are not a participant of this journey");
            }

            if (participant.Role == JourneyRole.Owner)
            {
                JourneyParticipant? newOwner = journey.Participants
                    .Where(p => p.UserId != userId && p.Status == RequestStatus.Accepted)
                    .OrderBy(p => p.JoinedAt)
                    .FirstOrDefault();

                if (newOwner != null)
                {
                    newOwner.Role = JourneyRole.Owner;
                    db.JourneyParticipants.Remove(participant);
                    await db.SaveChangesAsync();

                    logger.LogInformation("Ownership transferred from User {oldowner} to User {newownder}. {oldowner} has left the journey: {journey}.", userId, newOwner.UserId, userId, journeyId);
                    return ServiceResult.Succes();
                }
                db.Journeys.Remove(journey);
                await db.SaveChangesAsync();

                logger.LogInformation("Journey {journey} deleted as there were no participants.", journeyId);
                return ServiceResult.Succes($"Journey {journeyId} deleted as there were no participants.");                
            }

            db.JourneyParticipants.Remove(participant);
            await db.SaveChangesAsync();

            logger.LogInformation("User:{user} left Journey:{journey}", userId, journeyId);
            return ServiceResult.Succes();
        }

        public async Task<ServiceResult<List<PlaceDto>>> GetPlacesAsync()
        {
            try
            {
                var places = await db.Places
                    .AsNoTracking()
                    .OrderBy(p => p.City)
                    .ThenBy(p => p.District)
                    .ToListAsync();

                List<PlaceDto> dto = places.Select(p => new PlaceDto
                {
                    Id = p.Id,
                    City = p.City,
                    District = p.District,
                    CentreGPS = p.CentreGPS
                }).ToList();

                return ServiceResult<List<PlaceDto>>.Succes(dto);
            }
            catch (Exception ex)
            {
                logger.LogError("Error While Getting places: {error}", ex.Message);
                return ServiceResult<List<PlaceDto>>.Fail(ServiceResultStatus.Error, $"Error while loading places: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets Rating from a specific Journey, for a specific user
        /// </summary>
        public async Task<ServiceResult<RatingDto?>> GetMyRatingAsync(int userId, int journeyId)
        {
            // Check journey exists
            Journey? journey = await db.Journeys
                .Include(j => j.Ratings)
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
            {
                logger.LogWarning("GetMyRating failed, Journey {journey} not found", journeyId);
                return ServiceResult<RatingDto?>.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }

            // Check user is part of journey
            var participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                logger.LogWarning("GetMyRating failed, User {user} not in Journey {journey}", userId, journeyId);
                return ServiceResult<RatingDto?>.Fail(ServiceResultStatus.Unauthorized, "You are not part of this journey");
            }

            // Find rating
            Rating? rating = journey.Ratings.FirstOrDefault(r => r.UserId == userId);

            if (rating == null)
                return ServiceResult<RatingDto?>.Succes(null);

            return ServiceResult<RatingDto?>.Succes(new RatingDto {RatingValue = rating.RatingValue, Note = rating.Note});
        }

        /// <summary>
        /// Sets Rating from a specific Journey, for a specific user
        /// </summary>
        public async Task<ServiceResult> RateJourneyAsync(int userId, int journeyId, int ratingValue, string? note)
        {
            // Check user exists
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("RateJourney failed, User {user} not found", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            // Check journey exists + includes participants
            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .Include(j => j.Ratings)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
            {
                logger.LogWarning("RateJourney failed, Journey {journey} not found", journeyId);
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }

            // Check user is participant
            JourneyParticipant? participant = journey.Participants.FirstOrDefault(p => p.UserId == userId);

            if (participant == null)
            {
                logger.LogWarning("RateJourney failed, User {user} not participant of journey {journey}", userId, journeyId);
                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "You are not part of this journey");
            }

            // If user left → rating locked
            if (participant.Status == RequestStatus.Rejected)
            {
                return ServiceResult.Fail(ServiceResultStatus.InvalidOperation, "Cannot rate, you have left this journey");
            }

            // Try find existing rating
            Rating? rating = journey.Ratings
                .FirstOrDefault(r => r.UserId == userId && r.JourneyId == journeyId);

            if (rating == null)
            {
                // Create new rating
                rating = new Rating
                {
                    JourneyId = journeyId,
                    UserId = userId,
                    RatingValue = ratingValue,
                    Note = note,
                    Created = DateTime.UtcNow
                };

                db.Ratings.Add(rating);
            }
            else
            {
                // Update existing rating
                rating.RatingValue = ratingValue;
                rating.Note = note;
                rating.Created = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            logger.LogInformation("RateJourney success: User {user} rated Journey {journey}", userId, journeyId);

            return ServiceResult.Succes();
        }

    }
}
