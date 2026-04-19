using System.Security.Claims;

namespace Cryoptix.Observer.Authorization
{
    public interface IUserContextAccessor
    {
        string GetUserId(ClaimsPrincipal user);
        string? GetTenantId(ClaimsPrincipal user);
    }
}
