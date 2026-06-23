using Cryoptix.Strategy.Agent;

namespace Cryoptix.Strategy.Processor
{
    /// <summary>
    /// Defines the strategy processor contract.
    /// </summary>
    public interface IStrategyProcessor
    {
        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="strategyAgentSession">The strategy agent session.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ExecuteAsync(StrategyAgentSession strategyAgentSession, CancellationToken cancellationToken);
    }
}
