using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Status;
using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Controller
{
    public sealed class StrategyController(
        StrategyStateStore stateStore,
        IStrategyCommandQueue queue,
        IStrategyCatalog catalog) : IStrategyController
    {
        private readonly StrategyStateStore _strategyStateStore = stateStore;
        private readonly IStrategyCommandQueue _strategyCommandQueue = queue;
        private readonly IStrategyCatalog _strategyCatalog = catalog;

        public StrategyStatus GetStatus() => _strategyStateStore.Get();

        public IReadOnlyCollection<StrategyType> GetAvailableStrategies() => _strategyCatalog.Keys;

        public async Task<StrategyCommandResult> StartAsync(Runtime.Strategy strategy, CancellationToken ct)
        {
            if (!_strategyCatalog.TryCreate(strategy.StrategyType, out _))
            {
                return new StrategyCommandResult
                {
                    Success = false,
                    StatusCode = StrategyControllerStatusCodes.Status404NotFound,
                    Title = $"Strategy type '{strategy.StrategyType}' not found",
                    Message = $"Unknown strategy '{strategy.StrategyType}' {strategy.Name}"
                };
            }

            await _strategyCommandQueue.EnqueueAsync(new StrategyCommand
            {
                StrategyCommandType = StrategyCommandType.Start,
                Strategy = strategy
            }, ct);

            return new StrategyCommandResult
            {
                Success = true,
                StatusCode = StrategyControllerStatusCodes.Status202Accepted,
                Title = "Start command accepted.",
                Message = $"Start requested for strategy '{strategy.Name}'"
            };
        }

        public async Task<StrategyCommandResult> UpdateAsync(Runtime.Strategy strategy, CancellationToken ct)
        {

            await _strategyCommandQueue.EnqueueAsync(new StrategyCommand
            {
                StrategyCommandType = StrategyCommandType.Update,
                Strategy = strategy
            }, ct);

            return new StrategyCommandResult
            {
                Success = true,
                StatusCode = StrategyControllerStatusCodes.Status202Accepted,
                Title = "Update command accepted.",
                Message = $"Update requested for strategy '{strategy.Name}'"
            };
        }

        public async Task<StrategyCommandResult> StopAsync(CancellationToken ct)
        {
            await _strategyCommandQueue.EnqueueAsync(new StrategyCommand
            {
                StrategyCommandType = StrategyCommandType.Stop
            }, ct);

            return new StrategyCommandResult
            {
                Success = true,
                StatusCode = StrategyControllerStatusCodes.Status202Accepted,
                Title = "Stop command accepted.",
                Message = $"Stop requested"
            };
        }
    }
}
