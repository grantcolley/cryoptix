using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Dispatcher
{
    /// <summary>
    /// Defines the i strategy market event dispatcher contract.
    /// </summary>
    public interface IStrategyMarketEventDispatcher
    {
        Task DispatchAsync(
            StrategyProcessorSession session,
            MarketEvent marketEvent,
            Channel.StrategyEventChannels channels,
            CancellationToken cancellationToken);
    }
}
