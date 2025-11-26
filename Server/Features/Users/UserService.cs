using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Common;
using Server.Features.Buddies;
using Server.Features.Journeys;
using Server.Infrastructure.Data;
using Shared.Models.Dtos;
using Shared.Models.enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Server.Features.Users
{
    public class UserService: IUserService
    {
        private readonly AppDbContext db;
        private readonly string jwtSecret;
        private ILogger logger;
        public UserService(AppDbContext db, ILogger<UserService> logger, IConfiguration config)
        {            
            this.db = db;
            this.logger = logger;
            jwtSecret = config["JwtSettings:Secret"] ?? throw new Exception("JWT secret missing");
        }

        /// <summary>
        /// Registers a new user with the specified credentials and contact information.
        /// </summary>
        /// <param name="username">The username for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The email address for the new user.</param>
        /// <param name="phoneNumber">The phone number for the new user.</param>
        /// <returns> A <see cref="ServiceResult"/> indicating the success or failure of the registration process.</returns>
        public async Task<ServiceResult> Register(string username, string password, string email, string phoneNumber)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phoneNumber))
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Missing required data");

            if (await db.Users.AnyAsync(u => u.Username == username))
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, $"Username {username} already exists");

            if (!IsPasswordStrong(password))
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Weak password");

            User user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Email = email,
                Phonenumber = phoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return ServiceResult.Succes("User registered successfully");
        }

        /// <summary>
        /// Authenticates a user based on the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user attempting to log in</param>
        /// <param name="password">The password of the user attempting to log in</param>
        /// <returns> A <see cref="ServiceResult{T}"/> containing a JWT token if authentication is successful; otherwise, an error message.</returns>
        public async Task<ServiceResult<string>> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return ServiceResult<string>.Fail(ServiceResultStatus.ValidationError, "Username and password are required");

            User? user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return ServiceResult<string>.Fail(ServiceResultStatus.Unauthorized, "Invalid username or password");

            string token = GenerateJwtToken(user.Id, user.Username);
            return ServiceResult<string>.Succes(token);
        }


        /// <summary>
        /// Retrieves user information based on the specified user ID.
        /// </summary>
        /// <param name="Id">The unique identifier of the user to retrieve.</param>
        /// <returns> A <see cref="ServiceResult{T}"/> containing a <see cref="UserDto"/> with user information if found; otherwise, an error message.</returns>
        public async Task<ServiceResult<UserDto>> GetUserInfo(int id)
        {
            User? user = await db.Users.FindAsync(id);
            if (user == null)
                return ServiceResult<UserDto>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            return ServiceResult<UserDto>.Succes(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.Phonenumber,
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<ServiceResult<UserDto>> FindUserbyPhone(string number, int userId)
        {
            //todo: add more robust check for correct number
            if (string.IsNullOrWhiteSpace(number))
            {
                logger.LogInformation("FindUserbyPhone, Tried finding phonenumber but no number given");
                return ServiceResult<UserDto>.Fail(ServiceResultStatus.ValidationError, "phonenumber is required");
            }
                

            string digitsOnly = new string (number.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length < 8)
            {
                logger.LogInformation("FindUserbyPhone, Tried finding phonenumber {number}, but it has less then 8 chacters, so not a valid number", number);
                return ServiceResult<UserDto>.Fail(ServiceResultStatus.ValidationError, "phonenumber is not correct");
            }
                

            string last8 = digitsOnly.Substring(digitsOnly.Length - 8);

            User? user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    EF.Functions.Like(u.Phonenumber, "%" + last8));

            if (user == null)
            {
                logger.LogInformation("FindUserbyPhone, No user found with the following phonenumber: {Phonenumber}", number);
                return ServiceResult<UserDto>.Fail(ServiceResultStatus.UserNotFound, "Find not succesful");
            }
                

            bool isBlocked = await db.Buddys.AnyAsync(b =>
                (b.RequesterId == userId && b.AddresseeId == user.Id && b.Status == RequestStatus.Blocked) ||
                (b.AddresseeId == userId && b.RequesterId == user.Id && b.Status == RequestStatus.Blocked)
            );

            if (isBlocked)
            {
                logger.LogInformation("FindUserbyPhone, User {userId} tried finding blocked user with phonenumber {Phonenumber}, this is not allowed", userId, number);
                return ServiceResult<UserDto>.Fail(ServiceResultStatus.Blocked, "This user is blocked");
            }

            logger.LogDebug("FindUserbyPhone, user {userId} found the following User: {FoundUserId}", userId, user.Id);
            return ServiceResult<UserDto>.Succes(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.Phonenumber,
                CreatedAt = user.CreatedAt
            });
        }
        public async Task<ServiceResult> DeleteUserAsync(int userId)
        {
            // Check user exists
            User? user = await db.Users.FindAsync(userId);
            if (user == null)
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");

            using var transaction = await db.Database.BeginTransactionAsync();
           
            try
            {
                List<Buddy> buddies = await db.Buddys
                    .Where(b => b.RequesterId == userId || b.AddresseeId == userId)
                    .ToListAsync();

                BuddyService buddyService = new(db);
                foreach (var buddy in buddies)
                {
                    int buddyId = buddy.RequesterId == userId ? buddy.AddresseeId : buddy.RequesterId;                    
                    ServiceResult result = await buddyService.RemoveBuddy(userId, buddyId);
                    if (!result.IsSuccess)
                        return ServiceResult.Fail(result.Status, result.Message!);
                }

                List<int> journeyIds = await db.JourneyParticipants
                    .Where(p => p.UserId == userId)
                    .Select(p => p.JourneyId)
                    .Distinct()
                    .ToListAsync();

                if (journeyIds.Count > 0)
                {
                    JourneyService journeyService = new(db, buddyService, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<JourneyService>());

                    foreach (int journeyId in journeyIds)
                    {
                        ServiceResult result = await journeyService.LeaveJourneyAsync(userId, journeyId);
                        if (!result.IsSuccess)
                            return ServiceResult.Fail(result.Status, result.Message!);
                    }
                }

                db.Users.Remove(user);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Succes($"User {userId} deleted. Buddies removed and journeys updated.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Fail(ServiceResultStatus.Error, $"Failed to delete user: {ex.Message}");
            }
        }


        //---------------------- Helpers ----------------------
        /// <summary>
        /// Generate JWT token for a given user
        /// </summary>
        private string GenerateJwtToken(int id, string username)
        {
            JwtSecurityTokenHandler tokenHandler = new();

            Claim[] claims =
            [
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username)
            ];

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                    SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddHours(1)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //----------------------- Password Helpers ----------------------

        /// <summary>
        /// Determines whether the specified password meets the criteria for being considered strong.
        /// </summary>
        /// <param name="password">The password to evaluate for strength.</param>
        /// <returns><see langword="true"/> if the password is at least 8 characters long and contains at least one uppercase
        /// letter, one lowercase letter, one digit, and one special character; otherwise, <see langword="false"/>.</returns>
        public static bool IsPasswordStrong(string password)
        {
            if (password.Length < 8)
                return false;
            if (!password.Any(char.IsUpper))
                return false;
            if (!password.Any(char.IsLower))
                return false;
            if (!password.Any(char.IsDigit))
                return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;
            return true;
        }

        /// <summary>
        /// Generates a hashed representation of the specified password using PBKDF2 with SHA-256.
        /// </summary>
        /// <param name="password">The password to hash. Cannot be null or empty.</param>
        /// <returns>A base64-encoded string representing the hashed password, including the salt.</returns>
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies whether the specified password matches the stored hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="storedHash">The base64-encoded hash of the password, which includes the salt and the hashed password.</param>
        /// <returns><see langword="true"/> if the password matches the stored hash; otherwise, <see langword="false"/>.</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHash);
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                    {
                        return false;
                    }
                }
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }
    }
}
