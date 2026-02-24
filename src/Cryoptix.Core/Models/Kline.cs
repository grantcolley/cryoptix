using Cryoptix.Core.Enums;

namespace Cryoptix.Core.Models
{
    public class Kline
    {
        public string? Symbol { get; set; }
        public Exchange Exchange { get; set; }
        public KlineInterval Interval { get; set; }
        public DateTime OpenTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
        public DateTime CloseTime { get; set; }
        public bool Final { get; set; }
        public decimal QuoteAssetVolume { get; set; }
        public long NumberOfTrades { get; set; }
        public decimal TakerBuyBaseAssetVolume { get; set; }
        public decimal TakerBuyQuoteAssetVolume { get; set; }
    }
}