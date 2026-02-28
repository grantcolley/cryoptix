using Cryoptix.Core.Enums;

namespace Cryoptix.Exchange.Binance
{
    public static class OrderSideExtensions
    {
        public static global::Binance.Net.Enums.OrderSide ToBinanceOrderSide(this OrderSide order)
        {
            return order switch
            {
                OrderSide.Buy => global::Binance.Net.Enums.OrderSide.Buy,
                OrderSide.Sell => global::Binance.Net.Enums.OrderSide.Sell,
                _ => throw new NotImplementedException(),
            };
        }

        public static OrderSide ToCryoptixOrderSide(this global::Binance.Net.Enums.OrderSide order)
        {
            return order switch
            {
                global::Binance.Net.Enums.OrderSide.Buy => OrderSide.Buy,
                global::Binance.Net.Enums.OrderSide.Sell => OrderSide.Sell,
                _ => throw new NotImplementedException(),
            };
        }
    }
}