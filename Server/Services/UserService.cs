using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Server.Data;
using Server.Models;
using Shared.Models;

namespace Server.Services
{
    public class UserService
    {
        private readonly AppDbContext db;
        private readonly string jwtSecret;

        public UserService(AppDbContext db, IConfiguration config)
        {
            this.db = db;
            jwtSecret = config["JwtSettings:Secret"] ?? throw new Exception("JWT secret missing");
        }

        /// <summary>
        /// Register a new user with username and password.
        /// </summary>
        public async Task<UserDto> Register(string username, string password, string email, string phoneNumber)
        {
            if (db.Users.Any(u => u.Username == username))
                throw new Exception("Username already exists");
            if (!IsPasswordStrong(password))
                throw new Exception("Password does not meet strength requirements");

            User user = new User 
            { 
                Username = username, 
                PasswordHash = HashPassword(password), 
                Email = email, 
                Phonenumber = phoneNumber 
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.Phonenumber,
                CreatedAt = user.CreatedAt
            };
        }

        /// <summary>
        /// Login a user with username and password.
        /// Returns JWT token if successful, null otherwise.
        /// </summary>
        public string? Login(string username, string password)
        {
            User? user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return GenerateJwtToken(user.Id, user.Username);
        }


        /// <summary>
        /// Retrieves user information based on the specified user ID.
        /// </summary>
        public async Task<UserDto> GetUserInfo(int Id)
        {
            User? user = await db.Users.FindAsync(Id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return new UserDto 
            { 
                Id = user.Id, 
                Username = user.Username, 
                Email = user.Email, 
                PhoneNumber = user.Phonenumber, 
                CreatedAt = user.CreatedAt 
            };
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
        public bool IsPasswordStrong(string password)
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
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
    }
}
