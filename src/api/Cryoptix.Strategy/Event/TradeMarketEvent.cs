using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Event
{
    /// <summary>
    /// Represents the trade market event.
    /// </summary>
    public sealed class TradeMarketEvent(Trade trade) : MarketEvent
    {
        /// <summary>
        /// Gets the trade.
        /// </summary>
        public Trade Trade { get; } = trade ?? throw new ArgumentNullException(nameof(trade));
    }
}
