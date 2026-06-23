using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Dispatcher
{
    /// <summary>
    /// Defines the strategy market event dispatcher contract.
    /// </summary>
    public interface IStrategyMarketEventDispatcher
    {
        /// <summary>
        /// Dispatches the operation.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="marketEvent">The market event.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DispatchAsync(
            /// <summary>
            /// Gets the value.
            /// </summary>
            StrategyProcessorSession session,
            /// <summary>
            /// Gets the value.
            /// </summary>
            MarketEvent marketEvent,
            /// <summary>
            /// Gets the value.
            /// </summary>
            Channel.StrategyEventChannels channels,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken);
    }
}
