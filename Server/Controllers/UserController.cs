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
        private readonly IUserService service;
        private readonly ILogger logger;
        bool useSecureCookie;

        public UserController(IUserService service, ILogger<UserController> logger, IHostEnvironment env)
        {
            this.service = service;
            this.logger = logger;
            bool runningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

#if DEBUG
            useSecureCookie = false;
#else
            useSecureCookie = !env.IsDevelopment();
#endif
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (register == null)
            {
                logger.LogWarning("Register request body is null");
                return BadRequest("Required data missing");
            }

            ServiceResult result = await service.Register(register.Username, register.Password, register.Email, register.PhoneNumber);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("User {Username} registered successfully", register.Username);
                    return Ok($"User {register.Username} registered successfully");

                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("Registration failed: {Message}", result.Message);
                    return BadRequest(result.Message);

                default:
                    logger.LogError("Unknown registration error: {Message}", result.Message);
                    return StatusCode(500, "Unknown registration error");
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            ServiceResult<string> result = await service.Login(login.Username, login.Password);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    Response.Cookies.Append("jwt", result.Data!, new CookieOptions { HttpOnly = true, Secure = useSecureCookie, SameSite = SameSiteMode.Lax });
                    logger.LogInformation("User {Username} logged in successfully", login.Username);
                    return Ok(new { token = result.Data });

                case ServiceResultStatus.Unauthorized:
                    logger.LogWarning("Login failed for user {Username}: {Message}", login.Username, result.Message);
                    return Unauthorized(result.Message);

                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("Login validation failed for user {Username}: {Message}", login.Username, result.Message);
                    return BadRequest(result.Message);

                default:
                    logger.LogError("Unexpected login error for user {Username}: {Message}", login.Username, result.Message);
                    return StatusCode(500, "Unexpected login error");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("Verify")]
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
                    logger.LogDebug("Retrieved user info for UserID {UserId}", userId);
                    return Ok(result.Data);

                case ServiceResultStatus.UserNotFound:
                    logger.LogWarning("User not found: UserID {UserId}", userId);
                    return NotFound(result.Message);

                default:
                    logger.LogError("Unexpected error retrieving user info for UserID {UserId}: {Message}", userId, result.Message);
                    return StatusCode(500, "Unexpected error retrieving user info");
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            int userId = GetUserIdFromJwt();
            Response.Cookies.Delete("jwt");
            logger.LogInformation("User with User ID: {userId} logged out successfully", userId);
            return Ok("Logged out");
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("FindbyPhonenumber/{number}")]
        public async Task<IActionResult> FindbyPhoneNumber(string number)
        {
            int userId = GetUserIdFromJwt();
            ServiceResult<UserDto> result = await service.FindUserbyPhone(number, userId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Data);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Blocked:
                    return BadRequest(result.Message);
                default:
                    logger.LogError("FindbyPhoneNumber, user: {userId}, error: {message}, Number: {PhoneNumber}", userId, result.Message, number);
                    return StatusCode(500, "Unexpected error retrieving user info");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = GetUserIdFromJwt();
            ServiceResult result = await service.DeleteUserAsync(userId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("Delete User Succesful, Message: {message}", result.Message);
                    Logout();
                    return Ok();
                case ServiceResultStatus.UserNotFound:
                    logger.LogInformation("Delete User Failed, : {message}", result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                    logger.LogInformation("Delete User Failed, : {message}", result.Message);
                    return NotFound(result.Message);
                case ServiceResultStatus.InvalidOperation:
                    logger.LogInformation("Delete User Failed, : {message}", result.Message);
                    return BadRequest(result.Message);
                default:
                    logger.LogError("Delete User Failed, {message}", result.Message);
                    return StatusCode(500, "Unexpected error retrieving user info");
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
