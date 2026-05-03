using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Analysis
{
    public sealed class StrategyAnalysisContext
    {
        public Credentials? Credentials { get; init; }
        public required ExchangeApi ExchangeApi { get; init; }
        public required Strategies.Strategy Strategy { get; init; }
        public required IReadOnlyList<Kline> Klines { get; init; }
        public required IReadOnlyList<Trade> Trades { get; init; }
        public required MarketEventEnvelope CurrentEvent { get; init; }
        public required AccountRealtimeState AccountRealtimeState { get; init; }
        public required OrderBookRealtimeState OrderBookRealtimeState { get; init; }
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    }
}
