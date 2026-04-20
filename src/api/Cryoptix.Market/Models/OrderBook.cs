namespace Cryoptix.Market.Models
{
    public class OrderBook
    {
        public string? Symbol { get; set; }
        public Market.Models.Exchange Exchange { get; set; }
        public long LastUpdateId { get; set; }
        public DateTime UpdateTime { get; set; }
        public OrderBookPrice? BestAsk { get; set; }
        public OrderBookPrice? BestBid { get; set; }
        public IEnumerable<OrderBookPrice>? Bids { get; set; }
        public IEnumerable<OrderBookPrice>? Asks { get; set; }
    }
}
