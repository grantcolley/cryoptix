using Cryoptix.Core.Enums;

namespace Cryoptix.Core.Models
{
    public class ClientOrder
    {
        public Exchange Exchange { get; set; }
        public string? Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public TimeInForce TimeInForce { get; set; }
        public decimal StopPrice { get; set; }
    }
}
