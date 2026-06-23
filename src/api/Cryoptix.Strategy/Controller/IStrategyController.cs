using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.State;

namespace Cryoptix.Strategy.Controller
{
    /// <summary>
    /// Defines the strategy controller contract.
    /// </summary>
    public interface IStrategyController
    {
        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <returns>The get status result.</returns>
        StrategyStatus GetStatus();
        /// <summary>
        /// Gets the available strategies.
        /// </summary>
        /// <returns>The get available strategies result.</returns>
        IReadOnlyCollection<StrategyProcessorType> GetAvailableStrategies();
        /// <summary>
        /// Starts the operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<StrategyCommandResult> StartAsync(Strategies.Strategy strategy, CancellationToken ct);
        /// <summary>
        /// Updates the operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<StrategyCommandResult> UpdateAsync(Strategies.Strategy strategy, CancellationToken ct);
        /// <summary>
        /// Stops the operation.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<StrategyCommandResult> StopAsync(CancellationToken ct);
    }
}
