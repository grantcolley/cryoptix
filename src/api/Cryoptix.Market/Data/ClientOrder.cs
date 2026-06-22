namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the client order.
    /// </summary>
    public class ClientOrder
    {
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public string? Symbol { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public OrderType Type { get; set; }
        /// <summary>
        /// Gets or sets the side.
        /// </summary>
        public OrderSide Side { get; set; }
        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// Gets or sets the time in force.
        /// </summary>
        public TimeInForce TimeInForce { get; set; }
        /// <summary>
        /// Gets or sets the stop price.
        /// </summary>
        public decimal? StopPrice { get; set; }
    }
}
