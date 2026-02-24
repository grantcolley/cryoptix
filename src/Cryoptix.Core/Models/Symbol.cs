using Cryoptix.Core.Enums;

namespace Cryoptix.Core.Models
{
    public class Symbol
    {
        public string? Name { get; set; }
        public Exchange Exchange { get; set; }
        public string? NameDelimiter { get; set; }
        public string? ExchangeSymbol { get; set; }
        public Asset? BaseAsset { get; set; }
        public Asset? QuoteAsset { get; set; }
        public IEnumerable<OrderType>? OrderTypes { get; set; }
        public decimal NotionalMinimumValue { get; set; }
        public decimal TickSize { get; set; }
        public decimal LotSize { get; set; }
    }
}
