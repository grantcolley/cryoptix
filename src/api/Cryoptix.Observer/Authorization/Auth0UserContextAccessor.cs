using System.Security.Claims;

namespace Cryoptix.Observer.Authorization
{
    /// <summary>
    /// Represents the auth0 user context accessor.
    /// </summary>
    public sealed class Auth0UserContextAccessor : IUserContextAccessor
    {
        private const string TenantIdClaim = "https://your-company.example/tenant_id";

        /// <summary>
        /// Executes the get user id operation.
        /// </summary>
        /// <param name="user">The user value.</param>
        /// <returns>The get user id result.</returns>
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

        /// <summary>
        /// Executes the get tenant id operation.
        /// </summary>
        /// <param name="user">The user value.</param>
        /// <returns>The get tenant id result.</returns>
        public string? GetTenantId(ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return user.FindFirst(TenantIdClaim)?.Value;
        }
    }
}
