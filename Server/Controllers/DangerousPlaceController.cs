using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.Models.Dtos;
using System.Security.Claims;
using Server.Common;

namespace Server.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class DangerousPlaceController : ControllerBase
    {
        private readonly IDangerousPlaceService service;
        private readonly ILogger logger;
        public DangerousPlaceController(IDangerousPlaceService service, ILogger<DangerousPlaceController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpGet("GetMyReports")]
        public async Task<IActionResult> GetMyReports()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<List<DangerousPlaceDto>> result = await service.GetMyReportsAsync(userId);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                default:
                    logger.LogError("Unknown GetMyReports error: {Message}", result.Message);
                    return StatusCode(500, "Unknown GetMyReports error");
            }
        }

        [HttpPost("CreateReport")]
        public async Task<IActionResult> CreateReport([FromBody] DangerousPlaceCreateDto request)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.CreateReportAsync(userId, request);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Unauthorized(result.Message);
                default:
                    logger.LogError("Unknown CreateReport error: {Message}", result.Message);
                    return StatusCode(500, "Unknown CreateReport error");
            }
        }

        [HttpPatch("UpdateReport")]
        public async Task<IActionResult> UpdateReport([FromBody] DangerousPlaceCreateDto request)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.UpdateReportAsync(userId, request);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Unauthorized(result.Message);
                default:
                    logger.LogError("Unknown UpdateReport error: {Message}", result.Message);
                    return StatusCode(500, "Unknown UpdateReport error");
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