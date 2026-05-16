using Cryoptix.Market.Data;
using Cryoptix.Strategy.State;

namespace Cryoptix.Strategy.Cache
{
    public sealed class MarketDataSnapshot
    {
        public required StrategyState StrategyState { get; init; }
        public required Strategies.Strategy Strategy { get; init; }
        public required DateTime SnapshotTimeUtc { get; init; }
        public List<Kline> Klines { get; init; } = [];
        public List<Trade> Trades { get; init; } = [];
    }
}
