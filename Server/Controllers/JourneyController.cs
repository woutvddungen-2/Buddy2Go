using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
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
        public JourneyController(JourneyService service, ILogger<JourneyController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpGet("GetMyJourneys")]
        public async Task<IActionResult> GetMyJourneys()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<JourneyDto>> result = await service.GetJourneysByUserAsync(userId);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
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
                    return Ok(result.Data);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
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
                    return Ok(result.Data);
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Forbid(result.Message!);
                default:
                    logger.LogError("Error in GetBuddyJourneys for User:{User}, Journey:{journey}, Message:{message}",userId, journeyId, result.Message);
                    return StatusCode(500, "Error retrieving Journeys participants");
            }
        }

        [HttpPost("SendJoinRequest/{journeyId}")]
        public async Task<IActionResult> SendJoinRequest(int journeyId)
        {
            int userId = GetUserIdFromJwt();

            ServiceResult result = await service.SendJoinJourneyRequest(userId, journeyId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("User:{userId} sent join request to Journey:{journeyId}", userId, journeyId);
                    return Ok(result.Message);
                case ServiceResultStatus.UserNotFound:
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                case ServiceResultStatus.InvalidOperation:
                    return BadRequest(result.Message);
                default:
                    logger.LogError("SendJoinRequest unknown error for User:{userId}, Message:{message}", userId, result.Message);
                    return StatusCode(500, "Unexpected error occurred");
            }
        }


        [HttpPatch("RespondToJoinRequest/{journeyId}")]
        public async Task<IActionResult> RespondToJoinRequest( [FromBody] RequestResponseDto response, int journeyId)
        {
            int userId = GetUserIdFromJwt();

            ServiceResult result = await service.RespondToJourneyRequest(userId, journeyId, response.RequesterId, response.Status);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                case ServiceResultStatus.InvalidOperation:
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
            ServiceResult result = await service.AddJourneyAsync(userId, dto.StartGPS, dto.EndGPS, dto.StartAt);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
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
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Forbid();
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
                    return Ok();
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Forbid();
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
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
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.InvalidOperation:
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
