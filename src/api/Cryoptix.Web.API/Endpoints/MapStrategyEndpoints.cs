using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Controller;

namespace Cryoptix.Web.API.Endpoints
{
    public static class MapStrategyEndpoints
    {
        public static IEndpointRouteBuilder MapStrategyApi(this IEndpointRouteBuilder app)
        {
            var appGroup = app.MapGroup("/strategy");

            appGroup.MapGet("/strategies", GetStrategies);
            appGroup.MapGet("/status", GetStatus);
            appGroup.MapPost("/start", StartStrategy);
            appGroup.MapPost("/update", UpdateStrategy);
            appGroup.MapPost("/stop", StopStrategy);

            return app;
        }

        private static async Task<IResult> GetStrategies(
            IStrategyController strategyController)
        {
            return Results.Ok(strategyController.GetAvailableStrategies());
        }

        private static async Task<IResult> GetStatus(
            IStrategyController strategyController)
        {
            return Results.Ok(strategyController.GetStatus());
        }

        private static async Task<IResult> StartStrategy(
            Strategy.Runtime.Strategy strategy,
            IStrategyController strategyController,
            CancellationToken ct)
        {
            StrategyCommandResult result = await strategyController.StartAsync(strategy, ct);

            return result.Success
                ? Results.Accepted("/strategy/status", new { message = result.Message })
                : Results.Problem(
                    title: result.Title,
                    detail: result.Message,
                    statusCode: result.StatusCode);
        }

        private static async Task<IResult> UpdateStrategy(
            Strategy.Runtime.Strategy strategy,
            IStrategyController strategyController,
            CancellationToken ct)
        {
            StrategyCommandResult result = await strategyController.UpdateAsync(strategy, ct);

            return result.Success
                ? Results.Accepted("/strategy/status", new { message = result.Message })
                : Results.Problem(
                    title: result.Title,
                    detail: result.Message,
                    statusCode: result.StatusCode);
        }

        private static async Task<IResult> StopStrategy(
            IStrategyController strategyController,
            CancellationToken ct)
        {
            StrategyCommandResult result = await strategyController.StopAsync(ct);

            return result.Success
                ? Results.Accepted("/strategy/status", new { message = result.Message })
                : Results.Problem(
                    title: result.Title,
                    detail: result.Message,
                    statusCode: result.StatusCode);
        }
    }
}
