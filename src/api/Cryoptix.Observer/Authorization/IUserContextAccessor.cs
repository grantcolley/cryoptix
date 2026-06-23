using System.Security.Claims;

namespace Cryoptix.Observer.Authorization
{
    /// <summary>
    /// Defines the user context accessor contract.
    /// </summary>
    public interface IUserContextAccessor
    {
        /// <summary>
        /// Gets the user ID.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The get user ID result.</returns>
        string GetUserId(ClaimsPrincipal user);
        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The get tenant ID result.</returns>
        string? GetTenantId(ClaimsPrincipal user);
    }
}
