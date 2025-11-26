using Server.Common;
using Shared.Models.Dtos;

namespace Server.Features.Users
{
    public interface IUserService
    {
        Task<ServiceResult> Register(string username, string password, string email, string phoneNumber);
        Task<ServiceResult<string>> Login(string username, string password);
        Task<ServiceResult<UserDto>> GetUserInfo(int id);
        Task<ServiceResult<UserDto>> FindUserbyPhone(string number, int userId);
        Task<ServiceResult> DeleteUserAsync(int userId);
    }
}

