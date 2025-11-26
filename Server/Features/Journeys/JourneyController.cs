using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Models;
using Shared.Models.Dtos;
using System.Security.Claims;

namespace Server.Features.Journeys
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
            int userId = HttpContext.GetUserId();
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
            int userId = HttpContext.GetUserId();
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
            int userId = HttpContext.GetUserId();
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
            int userId = HttpContext.GetUserId();

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
            int userId = HttpContext.GetUserId();

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
            if (dto == null)
            {
                logger.LogWarning("Invalid journey data received in AddJourney.");
                return BadRequest("Invalid journey data.");
            }

            int userId = HttpContext.GetUserId();
            ServiceResult result = await service.AddJourneyAsync(userId, dto.StartPlaceId, dto.EndPlaceId, dto.StartAt);

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

            int userId = HttpContext.GetUserId();
            ServiceResult result = await service.UpdateJourneyGpsAsync(userId, JourneyId, dto.StartPlaceId, dto.EndPlaceId);

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
            int userId = HttpContext.GetUserId();
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
            int userId = HttpContext.GetUserId();
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

        [HttpPost("RateJourney/{journeyId}")]
        public async Task<IActionResult> RateJourney(int journeyId, [FromBody] RatingDto dto)
        {
            int userId = HttpContext.GetUserId();

            ServiceResult result = await service.RateJourneyAsync(userId, journeyId, dto.RatingValue, dto.Note);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.InvalidOperation:
                case ServiceResultStatus.Unauthorized:
                    return BadRequest(result.Message);
                default:
                    logger.LogError("RateJourney unknown error for User:{user}, Journey:{journey}, Msg:{message}",userId, journeyId, result.Message);
                    return StatusCode(500, "Unexpected error occurred");
            }
        }

        [HttpGet("GetMyRating/{journeyId}")]
        public async Task<IActionResult> GetMyRating(int journeyId)
        {
            int userId = HttpContext.GetUserId();

            ServiceResult<RatingDto?> result = await service.GetMyRatingAsync(userId, journeyId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Forbid(result.Message!);
                default:
                    logger.LogError("GetMyRating unknown error for User:{user}, Journey:{journey}, Msg:{message}",
                        userId, journeyId, result.Message);
                    return StatusCode(500, "Unexpected error occurred");
            }
        }



        [HttpGet("GetPlaces")]
        public async Task<IActionResult> GetPlaces()
        {
            ServiceResult<List<PlaceDto>> result = await service.GetPlacesAsync();
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.Error:
                    return StatusCode(500, result.Message);
                default:
                    logger.LogError("GetPlaces unknown error, Message:{message}", result.Message);
                    return StatusCode(500, result.Message);
            }
        }
    }
}
