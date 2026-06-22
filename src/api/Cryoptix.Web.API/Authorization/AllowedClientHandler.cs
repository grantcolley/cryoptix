using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Cryoptix.Web.API.Authorization
{
    internal sealed class AllowedClientHandler(IOptions<AuthOptions> options) : AuthorizationHandler<AllowedClientRequirement>
    {
        private readonly AuthOptions _authOptions = options.Value;

        /// <summary>
        /// Executes the handle requirement async operation.
        /// </summary>
        /// <param name="context">The context value.</param>
        /// <param name="requirement">The requirement value.</param>
        /// <returns>The handle requirement async result.</returns>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AllowedClientRequirement requirement)
        {
            var azp = context.User.FindFirst("azp")?.Value;

            if (azp is not null &&
                _authOptions.ClientIds.Contains(azp))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
