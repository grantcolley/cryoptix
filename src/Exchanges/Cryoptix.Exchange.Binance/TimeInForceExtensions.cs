using Cryoptix.Core.Enums;

namespace Cryoptix.Exchange.Binance
{
    public static class TimeInForceExtensions
    {
        public static global::Binance.Net.Enums.TimeInForce ToBinanceTimeInForce(this TimeInForce timeInForce)
        {
            return timeInForce switch
            {
                TimeInForce.FOK => global::Binance.Net.Enums.TimeInForce.FillOrKill,
                TimeInForce.GTC => global::Binance.Net.Enums.TimeInForce.GoodTillCanceled,
                TimeInForce.IOC => global::Binance.Net.Enums.TimeInForce.ImmediateOrCancel,
                _ => throw new NotImplementedException(),
            };
        }

        public static TimeInForce ToCryoptixTimeInForce(this global::Binance.Net.Enums.TimeInForce timeInForce)
        {
            return timeInForce switch
            {
                global::Binance.Net.Enums.TimeInForce.FillOrKill => TimeInForce.FOK,
                global::Binance.Net.Enums.TimeInForce.GoodTillCanceled => TimeInForce.GTC,
                global::Binance.Net.Enums.TimeInForce.ImmediateOrCancel => TimeInForce.IOC,
                _ => throw new NotImplementedException(),
            };
        }
    }
}