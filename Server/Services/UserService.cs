using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Server.Data;
using Server.Models;
using Shared.Models.Dtos;
using Microsoft.EntityFrameworkCore;

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
        /// Registers a new user with the specified credentials and contact information.
        /// </summary>
        /// <param name="username">The username for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The email address for the new user.</param>
        /// <param name="phoneNumber">The phone number for the new user.</param>
        /// <returns>A <see cref="RegisterResult"/> indicating the outcome of the registration process</returns>
        public async Task<RegisterResult> Register(string username, string password, string email, string phoneNumber)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phoneNumber))
                return RegisterResult.MissingData;
            if (await db.Users.AnyAsync(u => u.Username == username))
                return RegisterResult.UsernameExists;
            if (!IsPasswordStrong(password))
                return RegisterResult.WeakPassword;

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

            return RegisterResult.Success;
        }

        /// <summary>
        /// Authenticates a user based on the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user attempting to log in</param>
        /// <param name="password">The password of the user attempting to log in</param>
        /// <returns>A JSON Web Token (JWT) as a string if the authentication is successful; otherwise, <see langword="null"/>.</returns>
        public string? Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            User? user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return GenerateJwtToken(user.Id, user.Username);
        }


        /// <summary>
        /// Retrieves user information based on the specified user ID.
        /// </summary>
        /// <param name="Id">The unique identifier of the user to retrieve.</param>
        /// <returns>A <see cref="UserDto"/> containing the user's details, such as ID, username, email, phone number, and
        /// creation date.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no user is found with the specified <paramref name="Id"/>.</exception>
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
    //small enum to represent registration result
    public enum RegisterResult
    {
        Success,
        UsernameExists,
        WeakPassword,
        MissingData
    }
}
