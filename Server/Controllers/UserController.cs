using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Server.Services;
using Shared.Models.Dtos;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService service;
        ILogger logger;
        public UserController(UserService service)
        {
            this.service = service;
            logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UserController>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (register == null)
            {
                logger.LogWarning("Register request body is null");
                return BadRequest(new { message = "Required data missing" });
            }

            ServiceResult result = await service.Register(register.Username, register.Password, register.Email, register.PhoneNumber);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("User {Username} registered successfully", register.Username);
                    return Ok(new { message = $"User {register.Username} registered successfully" });

                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("Registration failed: {Message}", result.Message);
                    return BadRequest(new { message = result.Message });

                default:
                    logger.LogError("Unknown registration error: {Message}", result.Message);
                    return StatusCode(500, new { message = "Unknown registration error" });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            ServiceResult<string> result = await service.Login(login.Username, login.Password);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    Response.Cookies.Append("jwt", result.Data!, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax });
                    logger.LogInformation("User {Username} logged in successfully", login.Username);
                    return Ok(new { token = result.Data });

                case ServiceResultStatus.Unauthorized:
                    logger.LogWarning("Login failed for user {Username}: {Message}", login.Username, result.Message);
                    return Unauthorized(new { message = result.Message });

                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("Login validation failed for user {Username}: {Message}", login.Username, result.Message);
                    return BadRequest(new { message = result.Message });

                default:
                    logger.LogError("Unexpected login error for user {Username}: {Message}", login.Username, result.Message);
                    return StatusCode(500, new { message = "Unexpected login error" });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("verify")]
        public IActionResult Verify()
        {
            string username = User.Identity?.Name ?? "Unknown";
            string userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            if (!int.TryParse(userIdString, out int id))
            {
                logger.LogWarning("Invalid user ID in JWT: {userIdString}", userIdString);
                return BadRequest("Invalid user ID");
            }
            logger.LogDebug("JWT verified for user {username} with ID {id}", username, id);
            return Ok(new UserDto { Id = id, Username = username });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<UserDto> result = await service.GetUserInfo(userId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("Retrieved user info for UserID {UserId}", userId);
                    return Ok(result.Data);

                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("User not found: UserID {UserId}", userId);
                    return NotFound(new { message = result.Message });

                default:
                    logger.LogError("Unexpected error retrieving user info for UserID {UserId}: {Message}", userId, result.Message);
                    return StatusCode(500, new { message = "Unexpected error retrieving user info" });
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            int userId = GetUserIdFromJwt();
            Response.Cookies.Delete("jwt");
            logger.LogInformation("User with User ID: {userId} logged out successfully", userId);
            return Ok(new { message = "Logged out" });
        }

        // Helper method to extract user ID from JWT
        private int GetUserIdFromJwt()
        {
            Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
            return int.Parse(claim.Value);
        }
    }
}
