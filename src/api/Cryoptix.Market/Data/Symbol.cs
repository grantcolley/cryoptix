namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the symbol.
    /// </summary>
    public class Symbol
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the name delimiter.
        /// </summary>
        public string? NameDelimiter { get; set; }
        /// <summary>
        /// Gets or sets the exchange symbol.
        /// </summary>
        public string? ExchangeSymbol { get; set; }
        /// <summary>
        /// Gets or sets the base asset.
        /// </summary>
        public string? BaseAsset { get; set; }
        /// <summary>
        /// Gets or sets the base asset precision.
        /// </summary>
        public int BaseAssetPrecision { get; set; }
        /// <summary>
        /// Gets or sets the quote asset.
        /// </summary>
        public string? QuoteAsset { get; set; }
        /// <summary>
        /// Gets or sets the quote asset precision.
        /// </summary>
        public int QuoteAssetPrecision { get; set; }
        /// <summary>
        /// Gets or sets the notional minimum value.
        /// </summary>
        public decimal NotionalMinimumValue { get; set; }
        /// <summary>
        /// Gets or sets the tick size.
        /// </summary>
        public decimal TickSize { get; set; }
        /// <summary>
        /// Gets or sets the lot size.
        /// </summary>
        public decimal LotSize { get; set; }
    }
}
