using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Analysis
{
    /// <summary>
    /// Represents the strategy analysis context.
    /// </summary>
    public sealed class StrategyAnalysisContext
    {
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        public Credentials? Credentials { get; init; }
        /// <summary>
        /// Gets or sets the exchange api.
        /// </summary>
        public required ExchangeApi ExchangeApi { get; init; }
        /// <summary>
        /// Gets or sets the strategy.
        /// </summary>
        public required Strategies.Strategy Strategy { get; init; }
        /// <summary>
        /// Gets or sets the klines.
        /// </summary>
        public required IReadOnlyList<Kline> Klines { get; init; }
        /// <summary>
        /// Gets or sets the indicators.
        /// </summary>
        public required IReadOnlyList<Indicators> Indicators { get; init; }
        /// <summary>
        /// Gets or sets the trades.
        /// </summary>
        public required IReadOnlyList<Trade> Trades { get; init; }
        /// <summary>
        /// Gets or sets the current event.
        /// </summary>
        public required MarketEventEnvelope CurrentEvent { get; init; }
        /// <summary>
        /// Gets or sets the account realtime state.
        /// </summary>
        public required AccountRealtimeState AccountRealtimeState { get; init; }
        /// <summary>
        /// Gets or sets the order book realtime state.
        /// </summary>
        public required OrderBookRealtimeState OrderBookRealtimeState { get; init; }
        /// <summary>
        /// Gets or sets the timestamp utc.
        /// </summary>
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    }
}
