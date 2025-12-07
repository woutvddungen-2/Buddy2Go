using Server.Common;
using Shared.Models.Dtos.Users;
using Shared.Models.enums;
using Shared.Models.Enums;

namespace Server.Features.Users
{
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user with the specified credentials and contact information.
        /// </summary>
        /// <param name="username">The username for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The email address for the new user.</param>
        /// <param name="phoneNumber">The phone number for the new user.</param>
        /// <returns> A <see cref="ServiceResult"/> indicating the success or failure of the sms sending process.</returns>
        Task<ServiceResult> StartRegistrationAsync(string username, string password, string email, string phoneNumber);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phoneNumber">The phone number for the new user.</param>
        /// <param name="code">The Code that the user has send in for verification </param>
        /// <returns>A <see cref="ServiceResult"/> indicating the success or failure of the registration process.</returns>
        Task<ServiceResult> CompleteRegistrationAsync(string phoneNumber, string code);

        /// <summary>
        /// Authenticates a user based on the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user attempting to log in</param>
        /// <param name="password">The password of the user attempting to log in</param>
        /// <returns> A <see cref="ServiceResult{T}"/> containing a JWT token if authentication is successful; otherwise, an error message.</returns>
        Task<ServiceResult<string>> Login(string username, string password);

        /// <summary>
        /// Retrieves user information based on the specified user ID.
        /// </summary>
        /// <returns> A <see cref="ServiceResult{T}"/> containing a <see cref="UserDto"/> with user information if found; otherwise, an error message.</returns>
        Task<ServiceResult<UserDto>> GetUserInfo(int id);

        /// <summary>
        /// Retrives User Information based on the phonenumber of another user,
        /// only works if user is logged in, and found user has not blocked searching user (therefore, userId)
        /// </summary>
        /// <returns> A <see cref="ServiceResult{T}"/> Containing a <see cref="UserDto"/> With found user information, otherwise, an error message.</returns>
        Task<ServiceResult<UserDto>> FindUserbyPhone(string number, int userId);

        /// <summary>
        /// Deletes the user from the database, invokes LeaveJourney when user is part of a journey
        /// </summary>
        /// <returns>A <see cref="ServiceResult"/> indicating success or failure.</returns>
        Task<ServiceResult> DeleteUserAsync(int userId);

        /// <summary>
        /// Updates the user's email after validating the new email and ensuring it is unique.
        /// </summary>
        /// <returns>A <see cref="ServiceResult"/> indicating success or failure.</returns>
        Task<ServiceResult> UpdateEmailAsync(int userId, string newEmail);

        /// <summary>
        /// Updates the user's phone number after validating format and ensuring it is unique.
        /// </summary>
        /// <returns>A <see cref="ServiceResult"/> indicating success or failure.</returns>
        Task<ServiceResult> UpdatePhoneNumberAsync(int userId, string newPhoneNumber);

        /// <summary>
        /// Changes the user's password after validating the old password and checking password strength.
        /// </summary>
        /// <returns>A <see cref="ServiceResult"/> indicating success or failure.</returns>
        Task<ServiceResult> UpdatePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}

