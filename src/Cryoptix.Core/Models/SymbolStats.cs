using Cryoptix.Core.Enums;

namespace Cryoptix.Core.Models
{
    public class SymbolStats
    {
        public long FirstTradeId { get; set; }
        public DateTime CloseTime { get; set; }
        public DateTime OpenTime { get; set; }
        public decimal QuoteVolume { get; set; }
        public decimal Volume { get; set; }
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal BestAskQuantity { get; set; }
        public decimal BestAskPrice { get; set; }
        public decimal BestBidQuantity { get; set; }
        public decimal BestBidPrice { get; set; }
        public decimal LastQuantity { get; set; }
        public decimal LastPrice { get; set; }
        public decimal PreviousDayClosePrice { get; set; }
        public decimal WeightedAveragePrice { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal PriceChange { get; set; }
        public TimeSpan Period { get; set; }
        public string? Symbol { get; set; }
        public Exchange Exchange { get; set; }
        public long LastTradeId { get; set; }
        public long TotalTrades { get; set; }
    }
}
