using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.State;
using Cryoptix.Web.API.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Cryoptix.Web.API.Tests;

[TestClass]
public sealed class EndpointsTests
{
    [TestMethod]
    public void MapCryoptixApi_RegistersRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddAuthorization();
        builder.Services.AddSignalR();
        var queue = new Mock<IStrategyCommandQueue>(MockBehavior.Strict);
        var catalog = new Mock<IStrategyProcessorCatalog>(MockBehavior.Strict);
        var stateStore = new StrategyStateStore(NullLogger<StrategyStateStore>.Instance);
        builder.Services.AddSingleton<IStrategyCommandQueue>(queue.Object);
        builder.Services.AddSingleton<IStrategyProcessorCatalog>(catalog.Object);
        builder.Services.AddSingleton<StrategyStateStore>(stateStore);
        builder.Services.AddScoped<Strategy.Controller.IStrategyController, Strategy.Controller.StrategyController>();

        using WebApplication app = builder.Build();
        var returned = app.MapCryoptixApi();

        // Act
        string[] patterns = [.. ((Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)app).DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText!)];

        // Assert
        Assert.AreSame(app, returned);
        Assert.Contains("/", patterns);
        Assert.Contains("/health", patterns);
        Assert.Contains("/api/strategy/strategies", patterns);
        Assert.Contains("/api/strategy/status", patterns);
        Assert.Contains("/api/strategy/start", patterns);
        Assert.Contains("/api/strategy/update", patterns);
        Assert.Contains("/api/strategy/stop", patterns);
        Assert.Contains("/api/strategy/subscribe", patterns);
    }
}
