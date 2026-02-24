using Cryoptix.Core.Enums;

namespace Cryoptix.Core.Models
{
    public class Order
    {
        public string? AccountName { get; set; }
        public Exchange Exchange { get; set; }
        public string? Symbol { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? TransactTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? Id { get; set; }
        public string? ClientOrderId { get; set; }
        public decimal Price { get; set; }
        public decimal? AverageFillPrice { get; set; }
        public decimal? StopPrice { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal QuantityFilled { get; set; }
        public decimal QuantityRemaining { get; set; }
        public OrderStatus Status { get; set; }
        public TimeInForce TimeInForce { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public bool? IsWorking { get; set; }
    }
}