namespace Cryoptix.Market.Data
{
    public class Symbol
    {
        public string? Name { get; set; }
        public Exchange Exchange { get; set; }
        public string? NameDelimiter { get; set; }
        public string? ExchangeSymbol { get; set; }
        public string? BaseAsset { get; set; }
        public int BaseAssetPrecision { get; set; }
        public string? QuoteAsset { get; set; }
        public int QuoteAssetPrecision { get; set; }
        public decimal NotionalMinimumValue { get; set; }
        public decimal TickSize { get; set; }
        public decimal LotSize { get; set; }
    }
}
