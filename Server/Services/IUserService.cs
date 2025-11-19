using Server.Common;
using Shared.Models.Dtos;

namespace Server.Services
{
    public interface IUserService
    {
        Task<ServiceResult> Register(string username, string password, string email, string phoneNumber);
        Task<ServiceResult<string>> Login(string username, string password);
        Task<ServiceResult<UserDto>> GetUserInfo(int id);
        Task<ServiceResult<UserDto>> FindUserbyPhone(string number);
        Task<ServiceResult> DeleteUserAsync(int userId);
    }
}

