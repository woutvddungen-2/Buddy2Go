using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Models;
using Server.Services;
using Shared.Models.Dtos;
using System.Security.Claims;

namespace Server.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly JourneyService service;
        private readonly ILogger logger;
        public JourneyController(JourneyService service)
        {
            this.service = service;
            logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<JourneyController>();
        }

        [HttpGet("GetMyJourneys")]
        public async Task<IActionResult> GetMyJourneys()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<JourneyDto>> result = await service.GetJourneysByUserAsync(userId);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("GetJourneys, successfully retrieved journeys for User:{user}", userId);
                    return Ok(result.Data);
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("GetJourneys, user not found, User:{user}, Message:{message}", userId, result.Message);
                    return NotFound($"GetJourneys, user not found: {result.Message}");
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("GetJourneys, no journeys found for User:{user}, Message:{message}", userId, result.Message);
                    return NotFound($"No journeys found: {result.Message}");
                default:
                    logger.LogError("Unknown registration error: {Message}", result.Message);
                    return StatusCode(500, "Unknown registration error");
            }
        }

        [HttpGet("GetBuddyJourneys")]
        public async Task<IActionResult> GetBuddyJourneys()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<JourneyDto>> result = await service.GetBuddyJourneysAsync(userId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("GetBuddyJourneys, successfully retrieved for User:{user}", userId);
                    return Ok(result.Data);
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("GetBuddyJourneys, user not found: {user}", userId);
                    return NotFound($"User not found: {result.Message}");
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("GetJourneys, no journeys found for User:{user}, Message:{message}", userId, result.Message);
                    return NotFound($"No journeys found: {result.Message}");
                default:
                    logger.LogError("Error in GetBuddyJourneys for User:{user}, Message:{message}", userId, result.Message);
                    return StatusCode(500, "Error retrieving buddy journeys");
            }
        }

        [HttpGet("GetParticipants/{journeyId}")]
        public async Task<IActionResult> GetJourneyParticipants(int journeyId)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<JourneyParticipantDto>> result = await service.GetJourneyParticipantsAsync(journeyId, userId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("GetJourneyParticipants,successfully retrieved for User:{user}, journeyId:{journey}", userId, journeyId);
                    return Ok(result.Data);
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("GetJourneyParticipants, {message}, User:{User} JourneyId:{journey}", result.Message, userId, journeyId);
                    return NotFound($"GetJourneyParticipants, {result.Message}, JourneyId:{journeyId}");
                case ServiceResultStatus.Unauthorized:
                    logger.LogWarning("GetBuddyJourneys, unauthorized access attempt by User:{user}, in Journey:{Journey}", userId, journeyId);
                    return Forbid("Access denied.");
                default:
                    logger.LogError("Error in GetBuddyJourneys for User:{User}, Journey:{journey}, Message:{message}",userId, journeyId, result.Message);
                    return StatusCode(500, "Error retrieving Journeys participants");
            }
        }

        [HttpPost("JoinJourney/{journeyId}")]
        public async Task<IActionResult> JoinJourney(int journeyId)
        {
            int userId = GetUserIdFromJwt();

            ServiceResult result = await service.JoinJourneyAsync(userId, journeyId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("User:{userId} joined Journey:{journeyId}", userId, journeyId);
                    return Ok(result.Message);

                case ServiceResultStatus.UserNotFound:
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("JoinJourney failed for User:{userId}, Reason:{message}", userId, result.Message);
                    return NotFound(result.Message);

                case ServiceResultStatus.Unauthorized:
                case ServiceResultStatus.InvalidOperation:
                    logger.LogWarning("JoinJourney invalid for User:{userId}, Reason:{message}", userId, result.Message);
                    return BadRequest(result.Message);

                default:
                    logger.LogError("JoinJourney unknown error for User:{userId}, Message:{message}", userId, result.Message);
                    return StatusCode(500, "Unexpected error occurred");
            }
        }


        [HttpPost("AddJourney")]
        public async Task<IActionResult> AddJourney([FromBody] JourneyCreateDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.StartGPS) || string.IsNullOrEmpty(dto.EndGPS))
            {
                logger.LogWarning("Invalid journey data received in AddJourney.");
                return BadRequest("Invalid journey data.");
            }

            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.AddJourneyAsync(userId, dto.StartGPS, dto.EndGPS);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("AddJourney, journey added successfully for User:{user}", userId);
                    return Ok("Journey added successfully.");
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("AddJourney, user not found, User:{user}, Message:{message}", userId, result.Message);
                    return NotFound($"AddJourney, user not found: {result.Message}");
                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("AddJourney Parameter Error, User:{user}, Message:{message}", userId, result.Message);
                    return BadRequest($"AddJourney Parameter Error: {result.Message}");
                default:
                    logger.LogError("Unknown registration error: {Message}", result.Message);
                    return StatusCode(500, "Unknown registration error");
            }
        }

        [HttpPatch("UpdateGPS/{JourneyId}")]
        public async Task<IActionResult> UpdateJourney([FromBody] JourneyCreateDto dto, int JourneyId)
        {
            if (dto == null || JourneyId <= 0)
            {
                logger.LogWarning("Invalid journey data received in UpdateJourney.");
                return BadRequest("Invalid journey data.");
            }

            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.UpdateJourneyGpsAsync(userId, JourneyId, dto.StartGPS, dto.EndGPS);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("UpdateJourney, successfully updated journey for User:{user}, Journey:{journey}", userId, JourneyId);
                    return Ok("Journey updated successfully.");
                    case ServiceResultStatus.UserNotFound:
                        logger.LogWarning("UpdateJourney, user not found, User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                        return NotFound($"UpdateJourney, user not found: {result.Message}");
                    case ServiceResultStatus.ResourceNotFound:
                        logger.LogWarning("UpdateJourney, journey not found, User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                        return NotFound($"Journey not found: {result.Message}");
                    case ServiceResultStatus.ValidationError:
                        logger.LogWarning("UpdateJourney Parameter Error, User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                        return BadRequest($"UpdateJourney Parameter Error: {result.Message}");
                    case ServiceResultStatus.Unauthorized:
                        logger.LogWarning("UpdateJourney, unauthorized attempt, User:{user}, Journey:{journey}", userId, JourneyId);
                        return Forbid("Access denied.");
                default:
                    logger.LogError("UpdateJourney, error for User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                    return StatusCode(500, $"Error updating journey: {result.Message}");

            }
        }

        [HttpPatch("FinishJourney/{JourneyId}")]
        public async Task<IActionResult> FinishJourney(int JourneyId)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.FinishJourneyAsync(userId, JourneyId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("FinishJourney, successfully finished journey for User:{user}, Journey:{journey}", userId, JourneyId);
                    return Ok("Journey finished successfully.");
                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("FinishJourney Parameter Error, User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                    return BadRequest($"FinishJourney Parameter Error: {result.Message}");
                case ServiceResultStatus.Unauthorized:
                    logger.LogWarning("FinishJourney, unauthorized attempt, User:{user}, Journey:{journey}", userId, JourneyId);
                    return Forbid("Access denied.");
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("FinishJourney, journey not found, User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                    return NotFound($"Journey not found: {result.Message}");
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("FinishJourney, user not found, User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                    return NotFound($"FinishJourney, user not found: {result.Message}");
                default:
                    logger.LogError("FinishJourney, error for User:{user}, Journey:{journey}, Message:{message}", userId, JourneyId, result.Message);
                    return StatusCode(500, $"Error finishing journey: {result.Message}");
            }
        }

        [HttpDelete("LeaveJourney/{JourneyId}")]
        public async Task<IActionResult> LeaveJourney(int JourneyId)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.LeaveJourneyAsync(userId, JourneyId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("LeaveJourney, {message}", result.Message);
                    return Ok(result.Message);
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("LeaveJourney failed, User:{userId} not found, Message:{message}", userId, result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("LeaveJourney failed, Journey:{journeyId} not found for User:{userId}, Message:{message}", JourneyId, userId, result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.InvalidOperation:
                    logger.LogWarning("LeaveJourney invalid operation for User:{userId} on Journey:{journeyId}, Message:{message}", userId, JourneyId, result.Message);
                    return BadRequest(result.Message);
                default:
                    logger.LogError("LeaveJourney unknown error for User:{userId} on Journey:{journeyId}, Message:{message}", userId, JourneyId, result.Message);
                    return StatusCode(500, result.Message);
            }
        }


        // Helper method to extract user ID from JWT
        private int GetUserIdFromJwt()
        {
            Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
            return int.Parse(claim.Value);
        }


    }
}
