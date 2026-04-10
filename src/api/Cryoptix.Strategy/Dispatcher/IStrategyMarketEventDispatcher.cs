using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Dispatcher
{
    public interface IStrategyMarketEventDispatcher
    {
        Task DispatchAsync(
            StrategyProcessorSession session,
            MarketEvent marketEvent,
            CancellationToken cancellationToken);
    }
}
