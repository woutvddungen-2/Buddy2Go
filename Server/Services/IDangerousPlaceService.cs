using Server.Common;
using Shared.Models.Dtos;

namespace Server.Services
{
    public interface IDangerousPlaceService
    {
        public Task<ServiceResult<List<DangerousPlaceDto>>> GetMyReportsAsync(int userId);
        public Task<ServiceResult> CreateReportAsync(int userId, DangerousPlaceCreateDto report);
        public Task<ServiceResult> UpdateReportAsync(int userId, DangerousPlaceCreateDto report);

    }
}
