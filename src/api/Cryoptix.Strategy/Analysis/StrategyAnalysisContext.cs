using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
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
        public required IReadOnlyList<Market.Strategy.Indicators> Indicators { get; init; }
        /// <summary>
        /// Gets or sets the latest computed RSI snapshots for the strategy symbol
        /// across multiple periods. Each entry represents the most recent RSI state
        /// for a particular period. The collection may be empty when no RSI data is
        /// available for the symbol.
        /// </summary>
        public IReadOnlyList<Indicators.Rsi> Rsis { get; init; } = [];
        /// <summary>
        /// Gets or sets the latest computed MACD snapshots for the strategy symbol
        /// across multiple periods. Each entry represents the most recent MACD state
        /// for a particular period. The collection may be empty when no MACD data is
        /// available for the symbol.
        /// </summary>
        public IReadOnlyList<Indicators.Macd> Macds { get; init; } = [];
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
