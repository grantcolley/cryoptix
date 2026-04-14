using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Processor
{
    public sealed class StrategyProcessorSession
    {
        public required ExchangeApi ExchangeApi { get; init; }
        public required Runtime.Strategy Strategy { get; set; }
        public required MarketDataCache Cache { get; init; }
        public required OrderBookRealtimeState OrderBookRealtimeState { get; init; }
        public required AccountRealtimeState AccountRealtimeState { get; init; }
        public Credentials? Credentials { get; init; }
    }
}
