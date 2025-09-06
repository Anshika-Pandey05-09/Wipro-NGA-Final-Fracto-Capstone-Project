using System.Security.Claims;

namespace Fracto.Api.Infrastructure
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value;
            return int.TryParse(id, out var uid) ? uid : 0;
        }
    }
}
