using Cryoptix.Market.Data;

namespace Cryoptix.Market.Args
{
    /// <summary>
    /// Represents the order book event args.
    /// </summary>
    public class OrderBookEventArgs
    {
        /// <summary>
        /// Gets or sets the order book.
        /// </summary>
        public OrderBook? OrderBook { get; set; }
    }
}
