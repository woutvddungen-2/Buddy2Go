using Server.Features.Users;
using System.Security.Claims;

namespace Server.Common
{
    public static class HttpContextExtensions
    {
        public static int GetUserId(this HttpContext context)
        {
            string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("No user ID in JWT.");
            return int.Parse(userId);
        }
        public static string GetUsername(this HttpContext context)
        {
            string? userName = context.User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("No user ID in JWT.");
            return userName;
        }

        public static bool IsAuthenticated(this HttpContext context)
        {
            return context.User?.Identity?.IsAuthenticated ?? false;
        }

    }
}
