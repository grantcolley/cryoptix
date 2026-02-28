using Cryoptix.Core.Enums;

namespace Cryoptix.Exchange.Binance
{
    public static class OrderStatusExtensions
    {
        public static OrderStatus ToCryoptixOrderStatus(this global::Binance.Net.Enums.OrderStatus orderStatus)
        {
            return orderStatus switch
            {
                global::Binance.Net.Enums.OrderStatus.PendingNew => OrderStatus.PendingNew,
                global::Binance.Net.Enums.OrderStatus.New => OrderStatus.New,
                global::Binance.Net.Enums.OrderStatus.PartiallyFilled => OrderStatus.PartiallyFilled,
                global::Binance.Net.Enums.OrderStatus.Filled => OrderStatus.Filled,
                global::Binance.Net.Enums.OrderStatus.Canceled => OrderStatus.Canceled,
                global::Binance.Net.Enums.OrderStatus.PendingCancel => OrderStatus.PendingCancel,
                global::Binance.Net.Enums.OrderStatus.Rejected => OrderStatus.Rejected,
                global::Binance.Net.Enums.OrderStatus.Expired => OrderStatus.Expired,
                _ => OrderStatus.Unknown,
            };
        }
    }
}