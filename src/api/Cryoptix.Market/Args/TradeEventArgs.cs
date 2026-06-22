using Cryoptix.Market.Data;

namespace Cryoptix.Market.Args
{
    /// <summary>
    /// Represents the trade event args.
    /// </summary>
    public class TradeEventArgs
    {
        /// <summary>
        /// Gets or sets the trades.
        /// </summary>
        public IEnumerable<Trade>? Trades { get; set; }
    }
}