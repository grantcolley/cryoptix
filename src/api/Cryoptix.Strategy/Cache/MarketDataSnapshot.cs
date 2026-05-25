using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;

namespace Cryoptix.Strategy.Cache
{
    public sealed class MarketDataSnapshot
    {
        public required Strategies.Strategy Strategy { get; init; }
        public required DateTime SnapshotTimeUtc { get; init; }
        public List<Kline> Klines { get; init; } = [];
        public List<Trade> Trades { get; init; } = [];
        public List<Indicators> Indicators { get; init; } = [];
        public List<Market.Strategy.Signal> Signals { get; init; } = [];
    }
}
