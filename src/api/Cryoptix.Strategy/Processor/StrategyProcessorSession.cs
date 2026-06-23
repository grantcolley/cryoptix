using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Processor
{
    /// <summary>
    /// Represents the strategy processor session.
    /// </summary>
    public sealed class StrategyProcessorSession
    {
        /// <summary>
        /// Gets or sets the exchange api.
        /// </summary>
        public required ExchangeApi ExchangeApi { get; init; }
        /// <summary>
        /// Gets or sets the strategy.
        /// </summary>
        public required Strategies.Strategy Strategy { get; set; }
        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        public required MarketDataCache Cache { get; init; }
        /// <summary>
        /// Gets or sets the order book realtime state.
        /// </summary>
        public required OrderBookRealtimeState OrderBookRealtimeState { get; init; }
        /// <summary>
        /// Gets or sets the account realtime state.
        /// </summary>
        public required AccountRealtimeState AccountRealtimeState { get; init; }
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        public Credentials? Credentials { get; init; }
    }
}
