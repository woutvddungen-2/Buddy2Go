using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                return BadRequest(new { message = "Username and password are required" });
            }

            try
            {
                RegisterResult result = await service.Register(register.Username, register.Password, register.Email, register.PhoneNumber);
                switch (result)
                {
                    case RegisterResult.MissingData:
                        logger.LogInformation("Registration failed due to missing data for user");
                        return BadRequest(new { message = $"Registration failed due to missing data for user"});

                    case RegisterResult.UsernameExists:
                        logger.LogInformation("Registration failed: Username {Username} already exists", register.Username);
                        return BadRequest(new { message = $"Registration failed: Username {register.Username} already exists"});

                    case RegisterResult.WeakPassword:
                        logger.LogInformation("Registration failed: Weak password for user {Username}", register.Username);
                        return BadRequest(new { message = $"Registration failed: Weak password for user {register.Username}"});

                    case RegisterResult.Success:
                        logger.LogInformation("User {Username} registered successfully", register.Username);
                        return Ok(new { message = $"User {register.Username} registered successfully"});

                    default:
                        logger.LogError("Unknown registration error for user {Username}", register.Username);
                        return BadRequest(new { message = $"Unknown registration error for user {register.Username}"});
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("Error during registration for user {Username}: {ErrorMessage}", register.Username, ex.Message);
                return BadRequest(new { message = "Error during registration for user {Username}: {ErrorMessage}", register.Username, ex.Message });
            }
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            string? token = service.Login(login.Username, login.Password);
            if (token == null)
            {
                logger.LogWarning("Login failed for user {Username}", login.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

                Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
            });
            logger.LogInformation("User {Username} logged in successfully", login.Username);
            return Ok(new { token });
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
        [HttpGet("GetUserInfoFromDb")]
        public async Task<ActionResult<UserDto>> GetUserInfoFromDbAsync()
        {
            try
            {
                int userId = GetUserIdFromJwt();
                UserDto user = await service.GetUserInfo(userId);

                if (user == null)
                {
                    logger.LogWarning("User with ID {UserId} not found", userId);
                    return NotFound(new { message = "User not found" });
                }

                logger.LogInformation("Retrieved user info for user ID {UserId}", userId);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("Unauthorized access attempt: {ErrorMessage}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user info for user ID {UserId}", GetUserIdFromJwt());
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error" });
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
