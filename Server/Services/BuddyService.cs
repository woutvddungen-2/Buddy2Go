using Microsoft.EntityFrameworkCore;
using Server.Common;
using Server.Data;
using Server.Models;
using Shared.Models;
using Shared.Models.Dtos;

namespace Server.Services
{
    public class BuddyService
    {
        private readonly AppDbContext db;

        public BuddyService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ServiceResult> SendBuddyRequest(int requesterId, int addresseeId)
        {
            if (requesterId == addresseeId)
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Cannot add yourself as a buddy");

            // Check if users exist
            bool requesterExists = await db.Users.AnyAsync(u => u.Id == requesterId);
            bool addresseeExists = await db.Users.AnyAsync(u => u.Id == addresseeId);
            if (!requesterExists || !addresseeExists)
                return ServiceResult.Fail(ServiceResultStatus.UserNotFound, "User not found");

            Buddy? existingBuddy = await db.Buddys.FirstOrDefaultAsync(b =>
                (b.RequesterId == requesterId && b.AddresseeId == addresseeId) ||
                (b.RequesterId == addresseeId && b.AddresseeId == requesterId));

            if (existingBuddy != null)
            {
                switch (existingBuddy.Status)
                {
                    case RequestStatus.Blocked:
                        return ServiceResult.Fail(ServiceResultStatus.ValidationError, "You cannot send a buddy request to a blocked user");

                    case RequestStatus.Rejected:
                        existingBuddy.Status = RequestStatus.Pending;
                        existingBuddy.RequestedAt = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return ServiceResult.Succes("Buddy request re-sent successfully");

                    case RequestStatus.Pending:
                        return ServiceResult.Fail(ServiceResultStatus.ValidationError, "A buddy request is already pending");

                    case RequestStatus.Accepted:
                        return ServiceResult.Fail(ServiceResultStatus.ValidationError, "You are already buddies");
                }
            }

            Buddy buddy = new()
            {
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = RequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            await db.Buddys.AddAsync(buddy);
            await db.SaveChangesAsync();

            return ServiceResult.Succes("Buddy request sent");
        }

        public async Task<ServiceResult> RespondToBuddyRequest(int requesterId, int addresseeId, bool accept)
        {
            Buddy? buddy = await db.Buddys.FirstOrDefaultAsync(b =>
                b.RequesterId == requesterId && b.AddresseeId == addresseeId);

            if (buddy == null)
                return ServiceResult.Fail(ServiceResultStatus.ResourceNotFound, "Buddy request not found");
            if (buddy.Status != RequestStatus.Pending)
                return ServiceResult.Fail(ServiceResultStatus.ValidationError, "Buddy request already responded to");

            buddy.Status = accept ? RequestStatus.Accepted : RequestStatus.Rejected;
            await db.SaveChangesAsync();

            return ServiceResult.Succes($"Buddy request {(accept ? "accepted" : "rejected")}");
        }

        public async Task<ServiceResult<List<BuddyDto>>> GetBuddies(int userId)
        {
            bool userIdExists = await db.Users.AnyAsync(u => u.Id == userId);
            if (!userIdExists)
                return ServiceResult<List<BuddyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            List<BuddyDto> buddies = await db.Buddys
                .Where(b => (b.RequesterId == userId || b.AddresseeId == userId) && b.Status == RequestStatus.Accepted)
                .Select(b => new BuddyDto
                {
                    RequesterId = b.RequesterId,
                    AddresseeId = b.AddresseeId,
                    Status = b.Status,
                    RequestedAt = b.RequestedAt
                })
                .ToListAsync();

            return ServiceResult<List<BuddyDto>>.Succes(buddies);
        }

        public async Task<ServiceResult<List<BuddyDto>>> GetPendingRequests(int userId)
        {
            bool userIdExists = await db.Users.AnyAsync(u => u.Id == userId);
            if (!userIdExists)
                return ServiceResult<List<BuddyDto>>.Fail(ServiceResultStatus.UserNotFound, "User not found");

            List<BuddyDto> requests = await db.Buddys
                .Where(b => b.AddresseeId == userId && b.Status == RequestStatus.Pending)
                .Select(b => new BuddyDto
                {
                    RequesterId = b.RequesterId,
                    AddresseeId = b.AddresseeId,
                    Status = b.Status,
                    RequestedAt = b.RequestedAt
                })
                .ToListAsync();

            return ServiceResult<List<BuddyDto>>.Succes(requests);
        }
    }
}
