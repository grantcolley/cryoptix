using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Event
{
    /// <summary>
    /// Represents the market event envelope.
    /// </summary>
    public sealed record MarketEventEnvelope
    {
        /// <summary>
        /// Gets or sets the kind.
        /// </summary>
        public required MarketEventKind Kind { get; init; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public required MarketEventSource Source { get; init; }
        /// <summary>
        /// Gets or sets the kline.
        /// </summary>
        public Kline? Kline { get; init; }
        /// <summary>
        /// Gets or sets the trade.
        /// </summary>
        public Trade? Trade { get; init; }
    }
}
