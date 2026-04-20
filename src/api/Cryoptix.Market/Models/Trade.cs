namespace Cryoptix.Market.Models
{
    public class Trade
    {
        public string? Symbol { get; set; }
        public Market.Models.Exchange Exchange { get; set; }
        public DateTime Time { get; set; }
        public long Id { get; set; }
        public decimal Price { get; set; }
        public decimal BaseQuantity { get; set; }
        public decimal QuoteQuantity { get; set; }
    }
}