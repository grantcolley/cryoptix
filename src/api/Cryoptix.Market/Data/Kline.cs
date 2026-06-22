namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the kline.
    /// </summary>
    public class Kline
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
        /// Gets or sets the interval.
        /// </summary>
        public KlineInterval Interval { get; set; }
        /// <summary>
        /// Gets or sets the open time.
        /// </summary>
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Gets or sets the open.
        /// </summary>
        public decimal Open { get; set; }
        /// <summary>
        /// Gets or sets the high.
        /// </summary>
        public decimal High { get; set; }
        /// <summary>
        /// Gets or sets the low.
        /// </summary>
        public decimal Low { get; set; }
        /// <summary>
        /// Gets or sets the close.
        /// </summary>
        public decimal Close { get; set; }
        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Gets or sets the close time.
        /// </summary>
        public DateTime CloseTime { get; set; }
        /// <summary>
        /// Gets or sets the final.
        /// </summary>
        public bool Final { get; set; }
        /// <summary>
        /// Gets or sets the quote asset volume.
        /// </summary>
        public decimal QuoteAssetVolume { get; set; }
        /// <summary>
        /// Gets or sets the number of trades.
        /// </summary>
        public long NumberOfTrades { get; set; }
        /// <summary>
        /// Gets or sets the taker buy base asset volume.
        /// </summary>
        public decimal TakerBuyBaseAssetVolume { get; set; }
        /// <summary>
        /// Gets or sets the taker buy quote asset volume.
        /// </summary>
        public decimal TakerBuyQuoteAssetVolume { get; set; }
    }
}