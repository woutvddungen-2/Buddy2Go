using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Services;
using Shared.Models.Dtos;
using System.Security.Claims;
using System.Text;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BuddyController : ControllerBase
    {
        private readonly BuddyService service;
        private readonly ILogger logger;

        public BuddyController(BuddyService service)
        {
            this.service = service;
            logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BuddyController>();
        }


        [HttpPost("Send/{addresseeId}")]
        public async Task<IActionResult> SendBuddyRequest(int addresseeId)
        {
            int requesterId = GetUserIdFromJwt();
            ServiceResult result = await service.SendBuddyRequest(requesterId, addresseeId);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("SendBuddyRequest, successfully sent buddy request from User:{requester} to User:{addressee}", requesterId, addresseeId);
                    return Ok(result.Message);
                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("SendBuddyRequest, validation error for User:{requester} to User:{addressee}, Message:{message}", requesterId, addresseeId, result.Message);
                    return BadRequest(result.Message);
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("SendBuddyRequest, user not found for User:{requester} to User:{addressee}, Message:{message}", requesterId, addresseeId, result.Message);
                    return NotFound(result.Message);
                default:
                    logger.LogError("Unknown error in SendBuddyRequest from User:{requester} to User:{addressee}", requesterId, addresseeId);
                    return StatusCode(500, "Unknown error occurred");
            }
        }

        [HttpPatch("Respond")]
        public async Task<IActionResult> RespondToRequest([FromBody] RequestResponseDto request)
        {
            int addresseeId = GetUserIdFromJwt();
            ServiceResult result = await service.RespondToBuddyRequest(request.RequesterId, addresseeId, request.Status);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("RespondToBuddyRequest, User:{addressee} responded to buddy request from User:{requester} with Accept:{status}", addresseeId, request.RequesterId, request.Status);
                    return Ok(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("RespondToBuddyRequest, buddy request not found for User:{addressee} from User:{requester}, Message:{message}", addresseeId, request.RequesterId, result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("RespondToBuddyRequest, validation error for User:{addressee} from User:{requester}, Message:{message}", addresseeId, request.RequesterId, result.Message);
                    return BadRequest(result.Message);
                default:
                    logger.LogError("Unknown error in RespondToBuddyRequest for User:{addressee} from User:{requester}", addresseeId, request.RequesterId);
                    return StatusCode(500, "Unknown error occurred");
            }
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetBuddyList()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<BuddyDto>> result = await service.GetBuddies(userId);
            if (result.Status == ServiceResultStatus.UserNotFound)
            {
                logger.LogWarning("GetBuddyList, failed to retrieve buddies for User:{user}, Message:{message}", userId, result.Message);
                return StatusCode(500, "Failed to retrieve buddy list");
            }
            return Ok(result.Data);
        }

        [HttpGet("Pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<BuddyDto>> result = await service.GetPendingRequests(userId);
            if (result.Status == ServiceResultStatus.UserNotFound)
            {
                logger.LogWarning("GetBuddyList, failed to retrieve buddies for User:{user}, Message:{message}", userId, result.Message);
                return StatusCode(500, "Failed to retrieve buddy list");
            }
            return Ok(result.Data);
        }

        [HttpGet("GetSend")]
        public async Task<IActionResult> GetSend()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<BuddyDto>> result = await service.GetSendRequests(userId);
            if (result.Status == ServiceResultStatus.UserNotFound)
            {
                logger.LogWarning("GetBuddyList, failed to retrieve buddies for User:{user}, Message:{message}", userId, result.Message);
                return StatusCode(500, "Failed to retrieve buddy list");
            }
            return Ok(result.Data);
        }

        [HttpPatch("Block/{buddyId}")]
        public async Task<IActionResult> BlockBuddy(int buddyId)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.RemoveBuddy(userId, buddyId, true);
            switch (result.Status) {
                case ServiceResultStatus.Success:
                    logger.LogInformation("Blocked Buddy between user: {userId} and user: {buddyId}", userId, buddyId);
                    return Ok(result.Message);
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("Blockbuddy failes, {Message}", result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("Blockbuddy failes, {Message}", result.Message);
                    return NotFound(result.Message);
                default:
                    logger.LogError("Unknown error in Blockbuddy for User:{addressee} and User:{requester}", userId, buddyId);
                    return StatusCode(500, "Unknown error occurred");

            }
        }

        [HttpDelete("Delete/{buddyId}")]
        public async Task<IActionResult> DeleteBuddy(int buddyId)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.RemoveBuddy(userId, buddyId);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("Removed Buddy between user: {userId} and user: {buddyId}", userId, buddyId);
                    return Ok(result.Message);
                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("Removed buddy failes, {Message}", result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogWarning("Removed buddy failes, {Message}", result.Message);
                    return NotFound(result.Message);
                default:
                    logger.LogError("Unknown error in Removed buddy for User:{addressee} and User:{requester}", userId, buddyId);
                    return StatusCode(500, "Unknown error occurred");

            }
        }

        //------------------------ Helpers ----------------------------
        // Helper method to extract user ID from JWT
        private int GetUserIdFromJwt()
        {
            Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("Invalid JWT String");
            return int.Parse(claim.Value);
        }
    }
}
