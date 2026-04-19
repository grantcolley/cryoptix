using System.Security.Claims;

namespace Cryoptix.Observer.Authorization
{
    public sealed class Auth0UserContextAccessor : IUserContextAccessor
    {
        // Example Auth0 custom claim name. Adjust to match your token.
        // Auth0 recommends namespaced custom claims.
        private const string TenantIdClaim = "https://your-company.example/tenant_id";

        public string GetUserId(ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (user.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("The current user is not authenticated.");
            }

            var userId =
                user.FindFirst("sub")?.Value ??
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException(
                    "Authenticated user does not contain a usable subject claim ('sub' or NameIdentifier).");
            }

            return userId;
        }

        public string? GetTenantId(ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return user.FindFirst(TenantIdClaim)?.Value;
        }
    }
}
