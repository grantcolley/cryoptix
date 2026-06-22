namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the order.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        public string? AccountName { get; set; }
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public string? Symbol { get; set; }
        /// <summary>
        /// Gets or sets the created time.
        /// </summary>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// Gets or sets the transact time.
        /// </summary>
        public DateTime? TransactTime { get; set; }
        /// <summary>
        /// Gets or sets the update time.
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Gets or sets the client order id.
        /// </summary>
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Gets or sets the average fill price.
        /// </summary>
        public decimal? AverageFillPrice { get; set; }
        /// <summary>
        /// Gets or sets the stop price.
        /// </summary>
        public decimal? StopPrice { get; set; }
        /// <summary>
        /// Gets or sets the original quantity.
        /// </summary>
        public decimal OriginalQuantity { get; set; }
        /// <summary>
        /// Gets or sets the quantity filled.
        /// </summary>
        public decimal QuantityFilled { get; set; }
        /// <summary>
        /// Gets or sets the quantity remaining.
        /// </summary>
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the time in force.
        /// </summary>
        public TimeInForce TimeInForce { get; set; }
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public OrderType Type { get; set; }
        /// <summary>
        /// Gets or sets the side.
        /// </summary>
        public OrderSide Side { get; set; }
        /// <summary>
        /// Gets or sets the is working.
        /// </summary>
        public bool? IsWorking { get; set; }
    }
}