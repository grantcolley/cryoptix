using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Represents the market data snapshot.
    /// </summary>
    public sealed class MarketDataSnapshot
    {
        /// <summary>
        /// Gets or sets the strategy.
        /// </summary>
        public required Strategies.Strategy Strategy { get; init; }
        /// <summary>
        /// Gets or sets the snapshot time utc.
        /// </summary>
        public required DateTime SnapshotTimeUtc { get; init; }
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public required Symbol Symbol { get; init; }
        /// <summary>
        /// Gets or sets the klines.
        /// </summary>
        public List<Kline> Klines { get; init; } = [];
        /// <summary>
        /// Gets or sets the trades.
        /// </summary>
        public List<Trade> Trades { get; init; } = [];
        /// <summary>
        /// Gets or sets the indicators.
        /// </summary>
        public List<Indicators> Indicators { get; init; } = [];
        /// <summary>
        /// Gets or sets the signals.
        /// </summary>
        public List<Market.Strategy.Signal> Signals { get; init; } = [];
    }
}
