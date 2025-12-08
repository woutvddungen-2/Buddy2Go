using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common;
using Shared.Models.Dtos.Users;

namespace Server.Features.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService service;
        private readonly ILogger logger;
        bool sslEnabled;

        public UserController(IUserService service, ILogger<UserController> logger, IHostEnvironment env, IConfiguration config)
        {
            this.service = service;
            this.logger = logger;
            sslEnabled = config.GetRequiredSection("SSL:Enabled").Get<bool>();
        }

        [HttpPost("StartRegister")]
        public async Task<IActionResult> StartRegister([FromBody] RegisterDto register)
        {
            if (register == null)
            {
                logger.LogWarning("Register request body is null");
                return BadRequest("Required data missing");
            }

            ServiceResult result = await service.StartRegistrationAsync(register.Username, register.Password, register.Email, NormalizePhoneNumber(register.PhoneNumber));

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok($"User {register.Username} registered successfully");
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.Error:
                    return StatusCode(500, $"Error in sms module: {result.Message}");
                default:
                    logger.LogError("Unknown registration error: {Message}", result.Message);
                    return StatusCode(500, "Unknown registration error");
            }
        }

        [HttpPost("VerifyRegister")]
        public async Task<IActionResult> VerifyRegister([FromBody] VerifyUserDto verify)
        {
            if (verify == null)
            {
                logger.LogWarning("Register request body is null");
                return BadRequest("Required data missing");
            }

            ServiceResult result = await service.CompleteRegistrationAsync(NormalizePhoneNumber(verify.PhoneNumber), verify.Code);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Message);
                case ServiceResultStatus.ResourceNotFound:
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Unauthorized(result.Message);
                default:
                    logger.LogError("Unknown registration error: {Message}", result.Message);
                    return StatusCode(500, "Unknown registration error");
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (login == null)
            {
                logger.LogWarning("Login request body is null");
                return BadRequest("Login data missing");
            }

            ServiceResult<string> result = await service.Login(login.Identifier, login.Password);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    Response.Cookies.Append("jwt", result.Data!, new CookieOptions { HttpOnly = true, Secure = sslEnabled, SameSite = SameSiteMode.Lax });
                    logger.LogInformation("User {Username} logged in successfully", login.Identifier);
                    return Ok(new { token = result.Data });

                case ServiceResultStatus.Unauthorized:
                    logger.LogWarning("Login failed for user {Username}: {Message}", login.Identifier, result.Message);
                    return Unauthorized(result.Message);

                case ServiceResultStatus.ValidationError:
                    logger.LogWarning("Login validation failed for user {Username}: {Message}", login.Identifier, result.Message);
                    return BadRequest(result.Message);

                default:
                    logger.LogError("Unexpected login error for user {Username}: {Message}", login.Identifier, result.Message);
                    return StatusCode(500, "Unexpected login error");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("ReturnJWT")]
        public IActionResult Verify()
        {
            string username;
            int userId;
            try
            {
                username = HttpContext.GetUsername();
                userId = HttpContext.GetUserId();
            }
            catch (Exception ex)
            {
                logger.LogWarning("Invalid user in JWT: {ex}", ex.Message);
                return BadRequest($"Invalid user, {ex.Message}");
            }

            logger.LogDebug("JWT verified for user {username} with ID {id}", username, userId);
            return Ok(new UserDto { Id = userId, Username = username });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            int userId = HttpContext.GetUserId();
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
            int userId = HttpContext.GetUserId();
            Response.Cookies.Delete("jwt");
            logger.LogInformation("User with User ID: {userId} logged out successfully", userId);
            return Ok("Logged out");
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("FindbyPhonenumber/{number}")]
        public async Task<IActionResult> FindbyPhoneNumber(string number)
        {
            int userId = HttpContext.GetUserId();
            ServiceResult<UserDto> result = await service.FindUserbyPhone(NormalizePhoneNumber(number), userId);

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
        [HttpPatch("UpdateEmail/{email}")]
        public async Task<IActionResult> UpdateEmail(string email)
        {
            int userId = HttpContext.GetUserId();
            ServiceResult result = await service.UpdateEmailAsync(userId, email);
            switch (result.Status) {
                case ServiceResultStatus.Success:
                    return Ok(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                default:
                    logger.LogError("UpdateEmail, user: {userId}, error: {message}, Email: {Email}", userId, result.Message, email);
                    return StatusCode(500, "Unexpected error changing user info");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch("UpdatePhoneNumber/{number}")]
        public async Task<IActionResult> UpdatePhoneNumber(string number)
        {
            int userId = HttpContext.GetUserId();
            ServiceResult result = await service.UpdatePhoneNumberAsync(userId, NormalizePhoneNumber(number));
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                default:
                    logger.LogError("UpdatePhoneNumber, user: {userId}, error: {message}, Number: {PhoneNumber}", userId, result.Message, number);
                    return StatusCode(500, "Unexpected error changing user info");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null)
            {
                logger.LogWarning("ChangePassword dto is null");
                return BadRequest("ChangePassword dto data missing");
            }
            int userId = HttpContext.GetUserId();
            ServiceResult result = await service.UpdatePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    return Ok(result.Message);
                case ServiceResultStatus.ValidationError:
                    return BadRequest(result.Message);
                case ServiceResultStatus.UserNotFound:
                    return NotFound(result.Message);
                case ServiceResultStatus.Unauthorized:
                    return Unauthorized(result.Message);
                default:
                    logger.LogError("UpdatePassword, user: {userId}, error: {message}", userId, result.Message);
                    return StatusCode(500, "Unexpected error changing user info");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = HttpContext.GetUserId();
            ServiceResult result = await service.DeleteUserAsync(userId);

            switch (result.Status)
            {
                case ServiceResultStatus.Success:
                    logger.LogInformation("Delete User Succesful, Message: {message}", result.Message);
                    return Ok(result.Message);
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

        /// <summary>
        /// Normalizes Phoen Numbers, so users can use different style of input
        /// </summary>
        /// <param name="input">Phone Number</param>
        /// <returns>Normalizes phone number, to +31[Rest of phonenumber]</returns>
        private string NormalizePhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove all whitespace
            string cleaned = input.Replace(" ", "").Trim();

            // Case 1 — Already in +31 format
            if (cleaned.StartsWith("+31"))
            {
                // keep only digits after +31
                string rest = new string(cleaned.Substring(3).Where(char.IsDigit).ToArray());
                return $"+31{rest}";
            }

            // Case 2 — 0031 international format
            if (cleaned.StartsWith("0031"))
            {
                string rest = new string(cleaned.Substring(4).Where(char.IsDigit).ToArray());
                return $"+31{rest}";
            }

            // Remove everything except digits
            string digits = new string(cleaned.Where(char.IsDigit).ToArray());

            // Case 3 — Starts with 31 (mobile number without +)
            if (digits.StartsWith("31"))
            {
                string rest = digits.Substring(2); // remove 31
                return $"+31{rest}";
            }

            // Case 4 — Dutch "06..." mobile numbers
            if (digits.StartsWith("06"))
            {
                digits = digits.Substring(1); // remove only the first '0'
            }
            else if (digits.StartsWith("6"))
            {
                // handle "612345678" (missing leading 0)
                // nothing to strip
            }

            // Always return in +31 format
            return $"+31{digits}";
        }
    }
}
