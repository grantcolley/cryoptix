using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.State;

namespace Cryoptix.Strategy.Controller
{
    /// <summary>
    /// Represents the strategy controller.
    /// </summary>
    public sealed class StrategyController(
        StrategyStateStore stateStore,
        IStrategyCommandQueue queue,
        IStrategyProcessorCatalog strategyProcessorCatalog) : IStrategyController
    {
        private readonly StrategyStateStore _strategyStateStore = stateStore;
        private readonly IStrategyCommandQueue _strategyCommandQueue = queue;
        private readonly IStrategyProcessorCatalog _strategyProcessorCatalog = strategyProcessorCatalog;

        public StrategyStatus GetStatus() => _strategyStateStore.Get();

        /// <summary>
        /// Executes the get available strategies operation.
        /// </summary>
        /// <returns>The get available strategies result.</returns>
        public IReadOnlyCollection<StrategyProcessorType> GetAvailableStrategies() => _strategyProcessorCatalog.Keys;

        /// <summary>
        /// Executes the start async operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <param name="ct">The ct value.</param>
        /// <returns>The start async result.</returns>
        public async Task<StrategyCommandResult> StartAsync(Strategies.Strategy strategy, CancellationToken ct)
        {
            if (!_strategyProcessorCatalog.TryCreate(strategy.StrategyProcessorType, out _))
            {
                return new StrategyCommandResult
                {
                    Success = false,
                    StatusCode = StrategyControllerStatusCodes.Status404NotFound,
                    Title = $"Strategy processor '{strategy.StrategyProcessorType}' not found",
                    Message = $"Unknown strategy '{strategy.StrategyProcessorType}' {strategy.Name}"
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

        /// <summary>
        /// Executes the update async operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <param name="ct">The ct value.</param>
        /// <returns>The update async result.</returns>
        public async Task<StrategyCommandResult> UpdateAsync(Strategies.Strategy strategy, CancellationToken ct)
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

        /// <summary>
        /// Executes the stop async operation.
        /// </summary>
        /// <param name="ct">The ct value.</param>
        /// <returns>The stop async result.</returns>
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
