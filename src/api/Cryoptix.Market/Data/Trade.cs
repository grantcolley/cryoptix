namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the trade.
    /// </summary>
    public class Trade
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
        /// Gets or sets the time.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Gets or sets the base quantity.
        /// </summary>
        public decimal BaseQuantity { get; set; }
        /// <summary>
        /// Gets or sets the quote quantity.
        /// </summary>
        public decimal QuoteQuantity { get; set; }
    }
}