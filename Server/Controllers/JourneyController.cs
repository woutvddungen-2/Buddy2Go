using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using System.Security.Claims;

namespace Server.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly JourneyService service;
        ILogger logger;
        public JourneyController(JourneyService service)
        {
            this.service = service;
            logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<JourneyController>();
        }

        [HttpGet("GetJourneys")]
        public async Task<IActionResult> GetJourneys()
        {
            try
            {
                int userId = GetUserIdFromJwt();
                var journeys = await service.GetJourneysByUserAsync(userId);
                return Ok(journeys);
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Unauthorized access attempt to GetJourneys.");
                return Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while getting journeys.");
                return StatusCode(500, "Internal server error");
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
