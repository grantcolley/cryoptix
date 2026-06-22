using System.Security.Claims;

namespace Cryoptix.Observer.Authorization
{
    /// <summary>
    /// Defines the i user context accessor contract.
    /// </summary>
    public interface IUserContextAccessor
    {
        string GetUserId(ClaimsPrincipal user);
        string? GetTenantId(ClaimsPrincipal user);
    }
}
