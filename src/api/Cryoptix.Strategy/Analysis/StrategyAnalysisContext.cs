using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Event;

namespace Cryoptix.Strategy.Analysis
{
    public sealed class StrategyAnalysisContext
    {
        public required Runtime.Strategy Strategy { get; init; }
        public required ExchangeApi ExchangeApi { get; init; }

        public required IReadOnlyList<Kline> Klines { get; init; }
        public required IReadOnlyList<Trade> Trades { get; init; }

        public required MarketEventEnvelope CurrentEvent { get; init; }

        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    }
}
