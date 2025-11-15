using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Models;
using Server.Services;
using Shared.Models.Dtos;
using System.Security.Claims;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService chatService;
        ILogger<ChatController> logger;
        public ChatController(ChatService chatService, ILogger<ChatController> logger)
        {
            this.chatService = chatService;
            this.logger = logger;
        }

        [HttpGet("journey/{journeyId}")]
        public async Task<IActionResult> GetMessages(int journeyId)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<JourneyMessageDto>> result = await chatService.GetMessages(journeyId, userId);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Unauthorized(result.Message);
                default:
                    logger.LogError("Error in GetMessages for User:{User}, Journey:{journey}, Message:{message}", userId, journeyId, result.Message);
                    return StatusCode(500, "Error retrieving Journey Messages");
            }
        }

        [HttpPost("journey/{journeyId}")]
        public async Task<IActionResult> SendMessage(int journeyId, [FromBody] JourneyMessageCreateDto dto)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<JourneyMessageDto> result = await chatService.SendMessage(journeyId, userId, dto.Content);
            switch (result.Status)
            { 
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Unauthorized(result.Message);
                default:
                    logger.LogError("Error in SendMessage for User:{User}, Journey:{journey}, Message:{message}", userId, journeyId, result.Message);
                    return StatusCode(500, "Error sending Journey Messages");
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
