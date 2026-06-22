namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the fill.
    /// </summary>
    public class Fill
    {
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Gets or sets the commission.
        /// </summary>
        public decimal Commission { get; set; }
        /// <summary>
        /// Gets or sets the commission asset.
        /// </summary>
        public string? CommissionAsset { get; set; }
        /// <summary>
        /// Gets or sets the trade id.
        /// </summary>
        public long TradeId { get; set; }
    }
}
