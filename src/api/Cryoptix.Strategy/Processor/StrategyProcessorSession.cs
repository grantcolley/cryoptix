using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Cache;

namespace Cryoptix.Strategy.Processor
{
    public sealed class StrategyProcessorSession
    {
        public required ExchangeApi ExchangeApi { get; init; }
        public required Runtime.Strategy Strategy { get; set; }
        public required MarketDataCache Cache { get; init; }
    }
}
