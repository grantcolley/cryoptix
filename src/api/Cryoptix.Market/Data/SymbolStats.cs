namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the symbol stats.
    /// </summary>
    public class SymbolStats
    {
        /// <summary>
        /// Gets or sets the first trade id.
        /// </summary>
        public long FirstTradeId { get; set; }
        /// <summary>
        /// Gets or sets the close time.
        /// </summary>
        public DateTime CloseTime { get; set; }
        /// <summary>
        /// Gets or sets the open time.
        /// </summary>
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Gets or sets the quote volume.
        /// </summary>
        public decimal QuoteVolume { get; set; }
        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        public decimal Volume { get; set; }
        /// <summary>
        /// Gets or sets the low price.
        /// </summary>
        public decimal LowPrice { get; set; }
        /// <summary>
        /// Gets or sets the high price.
        /// </summary>
        public decimal HighPrice { get; set; }
        /// <summary>
        /// Gets or sets the open price.
        /// </summary>
        public decimal OpenPrice { get; set; }
        /// <summary>
        /// Gets or sets the best ask quantity.
        /// </summary>
        public decimal BestAskQuantity { get; set; }
        /// <summary>
        /// Gets or sets the best ask price.
        /// </summary>
        public decimal BestAskPrice { get; set; }
        /// <summary>
        /// Gets or sets the best bid quantity.
        /// </summary>
        public decimal BestBidQuantity { get; set; }
        /// <summary>
        /// Gets or sets the best bid price.
        /// </summary>
        public decimal BestBidPrice { get; set; }
        /// <summary>
        /// Gets or sets the last quantity.
        /// </summary>
        public decimal LastQuantity { get; set; }
        /// <summary>
        /// Gets or sets the last price.
        /// </summary>
        public decimal LastPrice { get; set; }
        /// <summary>
        /// Gets or sets the previous day close price.
        /// </summary>
        public decimal PreviousDayClosePrice { get; set; }
        /// <summary>
        /// Gets or sets the weighted average price.
        /// </summary>
        public decimal WeightedAveragePrice { get; set; }
        /// <summary>
        /// Gets or sets the price change percent.
        /// </summary>
        public decimal PriceChangePercent { get; set; }
        /// <summary>
        /// Gets or sets the price change.
        /// </summary>
        public decimal PriceChange { get; set; }
        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public TimeSpan Period { get; set; }
        /// <summary>
        /// Gets or sets the symbol.
        /// </summary>
        public string? Symbol { get; set; }
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the last trade id.
        /// </summary>
        public long LastTradeId { get; set; }
        /// <summary>
        /// Gets or sets the total trades.
        /// </summary>
        public long TotalTrades { get; set; }
    }
}
