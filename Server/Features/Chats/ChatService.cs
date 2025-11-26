using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Features.Journeys;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Shared.Models.Dtos;
using Shared.Models.enums;

namespace Server.Features.Chats
{
    public class ChatService
    {
        private readonly AppDbContext db;
        private ILogger logger;

        public ChatService(AppDbContext db, ILogger<ChatService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// Get Messages of a Journey
        /// </summary>
        public async Task<ServiceResult<List<JourneyMessageDto>>> GetMessages(int journeyId, int userId)
        {
            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
            {
                logger.LogWarning("GetMessages, Journey {journeyId} not found", journeyId);
                return ServiceResult<List<JourneyMessageDto>>.Fail(ServiceResultStatus.ResourceNotFound, $"Journey not Found");
            }                

            if (!journey.Participants.Any(p => p.UserId == userId && p.Status == RequestStatus.Accepted))
            {
                logger.LogWarning("GetMessages, user {userId} is not part of Journey: {journeyId}", userId, journeyId);
                return ServiceResult<List<JourneyMessageDto>>.Fail(ServiceResultStatus.Unauthorized, "You are not part of this journey");
            }

            List<JourneyMessageDto> messages = await db.JourneyMessages
                .Where(m => m.JourneyId == journeyId)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .Select(m => new JourneyMessageDto
                {
                    Id = m.Id,
                    JourneyId = m.JourneyId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Username,
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            logger.LogTrace("GetMessages, succesfully got messages for user {UserId}, from Journey {JourneyId}", userId, journeyId);
            return ServiceResult<List<JourneyMessageDto>>.Succes(messages);
        }

        /// <summary>
        /// send a message in the Journey Groupchat
        /// </summary>
        public async Task<ServiceResult<JourneyMessageDto>> SendMessage(int journeyId, int userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogWarning("SendMessage, message parameter cannot be empty");
                return ServiceResult<JourneyMessageDto>.Fail(ServiceResultStatus.ValidationError, "Message cannot be empty");
            }

            Journey? journey = await db.Journeys
                .Include(j => j.Participants)
                .FirstOrDefaultAsync(j => j.Id == journeyId);

            if (journey == null)
            {
                logger.LogWarning("SendMessage, Journey {journeyId} not found", journeyId);
                return ServiceResult<JourneyMessageDto>.Fail(ServiceResultStatus.ResourceNotFound, "Journey not found");
            }


            if (!journey.Participants.Any(p => p.UserId == userId && p.Status == RequestStatus.Accepted))
            {
                logger.LogWarning("SendMessage, user {userId} is not part of journey {journeyId}", userId, journeyId);
                return ServiceResult<JourneyMessageDto>.Fail(ServiceResultStatus.Unauthorized, "You are not part of this journey");
            }
            JourneyMessage message = new JourneyMessage
            {
                JourneyId = journeyId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            await db.JourneyMessages.AddAsync(message);
            await db.SaveChangesAsync();

            User? sender = await db.Users.FindAsync(userId);
            if (sender == null)
            {
                logger.LogError("SendMessage: Sender not found in database, UserId {UserId}", userId);
                return ServiceResult<JourneyMessageDto>.Fail(ServiceResultStatus.Error, "Unexpected error: sender not found");
            }

            logger.LogTrace("SendMessage, succesfully send message from user {UserId}, to Journey {JourneyId}", userId, journeyId);
            return ServiceResult<JourneyMessageDto>.Succes(new JourneyMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = sender.Username,
                Content = message.Content,
                SentAt = message.SentAt
            });
        }
    }
}
