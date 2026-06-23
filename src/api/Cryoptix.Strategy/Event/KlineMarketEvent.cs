using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Event
{
    /// <summary>
    /// Represents the kline market event.
    /// </summary>
    public sealed class KlineMarketEvent(Kline kline, MarketEventSource source) : MarketEvent
    {
        /// <summary>
        /// Gets or sets the kline.
        /// </summary>
        public Kline Kline { get; } = kline ?? throw new ArgumentNullException(nameof(kline));
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public MarketEventSource Source { get; } = source;
    }
}
