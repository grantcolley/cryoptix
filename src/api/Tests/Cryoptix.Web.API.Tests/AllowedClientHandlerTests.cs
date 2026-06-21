using Cryoptix.Web.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Cryoptix.Web.API.Tests;

[TestClass]
public sealed class AllowedClientHandlerTests
{
    [TestMethod]
    public async Task SucceedsWhenAzpClaimIsAllowed()
    {
        // Arrange
        var handler = new AllowedClientHandler(Options.Create(new AuthOptions { ClientIds = ["client-1"] }));
        var requirement = new AllowedClientRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("azp", "client-1")], "test"));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.IsTrue(context.HasSucceeded);
    }

    [TestMethod]
    public async Task FailureWhenAzpClaimIsMissingOrUnknown()
    {
        // Arrange
        var handler = new AllowedClientHandler(Options.Create(new AuthOptions { ClientIds = ["client-1"] }));
        var requirement = new AllowedClientRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("azp", "other")], "test"));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.IsFalse(context.HasSucceeded);
    }
}
