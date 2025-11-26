using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Infrastructure.Data;
using Shared.Models.Dtos;
using Shared.Models.enums;

namespace Server.Features.DangerousPlaces
{
    public class DangerousPlaceService : IDangerousPlaceService
    {
        private readonly AppDbContext db;
        private ILogger logger;

        public DangerousPlaceService(AppDbContext db, ILogger<DangerousPlaceService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        /// <summary>
        /// Gets all Dangerous Place reports for a single user
        /// </summary>
        public async Task<ServiceResult<List<DangerousPlaceDto>>> GetMyReportsAsync(int userId)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("GetMyReports Failed, User: {user} not found", userId);
                return ServiceResult<List<DangerousPlaceDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            List<DangerousPlaceDto> reports = await db.DangerousPlaces
                .Where(u => u.ReportedById == userId && u.ReportedAt > DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(p => p.ReportedAt)
                .Select(p => new DangerousPlaceDto
                {
                    Id = p.Id,
                    ReportedById = p.ReportedById,
                    PlaceType = p.PlaceType,
                    Description = p.Description,
                    GPS = p.GPS,
                    ReportedAt = p.ReportedAt
                })
                .ToListAsync();

            logger.LogDebug("GetMyReports, {count} reports succesfully retrieved for user {userId}", reports.Count, userId);
            return ServiceResult<List<DangerousPlaceDto>>.Succes(reports);
        }

        /// <summary>
        /// Creates a Dangerous Place report
        /// </summary>
        public async Task<ServiceResult> CreateReportAsync(int userId, DangerousPlaceCreateDto report)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("CreateReportAsync Failed, User: {user} not found", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            if (string.IsNullOrWhiteSpace(report.GPS))
            {
                logger.LogWarning("CreateReportAsync Failed, GPS location empty.}");
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "GPS location is required.");
            }

            DangerousPlace place = new DangerousPlace
            {
                ReportedById = userId,
                PlaceType = report.PlaceType,
                Description = report.Description?.Trim() ?? string.Empty,
                GPS = report.GPS.Trim(),
                ReportedAt = DateTime.UtcNow
            };

            db.DangerousPlaces.Add(place);
            await db.SaveChangesAsync();

            logger.LogInformation("Dangerous place {placeId} created by user {userId} at {gps}", report.Id, userId, report.GPS);
            return ServiceResult.Succes();
        }

        /// <summary>
        /// Updates an already existing Dangerous Place report
        /// </summary>
        public async Task<ServiceResult> UpdateReportAsync(int userId, DangerousPlaceCreateDto report)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId))
            {
                logger.LogWarning("UpdateReportAsync Failed, User: {user} not found", userId);
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");
            }

            DangerousPlace? existing = await db.DangerousPlaces.FirstOrDefaultAsync(p => p.Id == report.Id);

            if (existing == null)
            {
                logger.LogWarning("UpdateReportAsync: dangerous place {id} not found", report.Id);
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Dangerous place not found.");
            }
            if (existing.ReportedById != userId)
            {
                logger.LogWarning("UpdateReportAsync: user {userId} tried to edit report {id} owned by {ownerId}", userId, existing.Id, existing.ReportedById);

                return ServiceResult.Fail(ServiceResultStatus.Unauthorized, "You can only edit your own reports.");
            }
            if (existing.ReportedAt < DateTime.UtcNow.AddDays(-7))
            {
                logger.LogWarning("UpdateReportAsync: report {id} cannot be edited anymore (older than 7 days).", report.Id);
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Report can only be edited within 7 days.");
            }

            existing.PlaceType = report.PlaceType;
            existing.GPS = report.GPS.Trim();
            if (!string.IsNullOrEmpty(existing.Description))
                existing.Description = existing.Description.Trim();
            else
                existing.Description = null;

            DangerousPlace place = new DangerousPlace
            {
                ReportedById = userId,
                PlaceType = report.PlaceType,
                Description = report.Description?.Trim() ?? string.Empty,
                GPS = report.GPS.Trim(),
                ReportedAt = DateTime.UtcNow
            };

            db.DangerousPlaces.Add(place);
            await db.SaveChangesAsync();

            logger.LogInformation("Dangerous place {placeId} created by user {userId} at {gps}", report.Id, userId, report.GPS);
            return ServiceResult.Succes();
        }
    }
}
