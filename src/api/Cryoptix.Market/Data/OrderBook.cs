namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the order book.
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public string? Symbol { get; set; }
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the last update id.
        /// </summary>
        public long LastUpdateId { get; set; }
        /// <summary>
        /// Gets or sets the update time.
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// Gets or sets the best ask.
        /// </summary>
        public OrderBookPrice? BestAsk { get; set; }
        /// <summary>
        /// Gets or sets the best bid.
        /// </summary>
        public OrderBookPrice? BestBid { get; set; }
        /// <summary>
        /// Gets or sets the bids.
        /// </summary>
        public IEnumerable<OrderBookPrice>? Bids { get; set; }
        /// <summary>
        /// Gets or sets the asks.
        /// </summary>
        public IEnumerable<OrderBookPrice>? Asks { get; set; }
    }
}
