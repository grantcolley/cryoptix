using Cryoptix.Core.Enums;

namespace Cryoptix.Exchange.Binance
{
    public static class KlineIntervalExtension
    {
        public static global::Binance.Net.Enums.KlineInterval ToKlineInterval(this KlineInterval klineInterval)
        {
            return klineInterval switch
            {
                KlineInterval.Minute => global::Binance.Net.Enums.KlineInterval.OneMinute,
                KlineInterval.Minutes3 => global::Binance.Net.Enums.KlineInterval.ThreeMinutes,
                KlineInterval.Minutes5 => global::Binance.Net.Enums.KlineInterval.FiveMinutes,
                KlineInterval.Minutes15 => global::Binance.Net.Enums.KlineInterval.FifteenMinutes,
                KlineInterval.Minutes30 => global::Binance.Net.Enums.KlineInterval.ThirtyMinutes,
                KlineInterval.Hour => global::Binance.Net.Enums.KlineInterval.OneHour,
                KlineInterval.Hours2 => global::Binance.Net.Enums.KlineInterval.TwoHour,
                KlineInterval.Hours4 => global::Binance.Net.Enums.KlineInterval.FourHour,
                KlineInterval.Hours6 => global::Binance.Net.Enums.KlineInterval.SixHour,
                KlineInterval.Hours8 => global::Binance.Net.Enums.KlineInterval.EightHour,
                KlineInterval.Hours12 => global::Binance.Net.Enums.KlineInterval.TwelveHour,
                KlineInterval.Day => global::Binance.Net.Enums.KlineInterval.OneDay,
                KlineInterval.Week => global::Binance.Net.Enums.KlineInterval.OneWeek,
                _ => throw new NotImplementedException(),
            };
        }

        public static KlineInterval ToCryoptixKlineInterval(this global::Binance.Net.Enums.KlineInterval klineInterval)
        {
            return klineInterval switch
            {
                global::Binance.Net.Enums.KlineInterval.OneMinute => KlineInterval.Minute,
                global::Binance.Net.Enums.KlineInterval.ThreeMinutes => KlineInterval.Minutes3,
                global::Binance.Net.Enums.KlineInterval.FiveMinutes => KlineInterval.Minutes5,
                global::Binance.Net.Enums.KlineInterval.FifteenMinutes => KlineInterval.Minutes15,
                global::Binance.Net.Enums.KlineInterval.ThirtyMinutes => KlineInterval.Minutes30,
                global::Binance.Net.Enums.KlineInterval.OneHour => KlineInterval.Hour,
                global::Binance.Net.Enums.KlineInterval.TwoHour => KlineInterval.Hours2,
                global::Binance.Net.Enums.KlineInterval.FourHour => KlineInterval.Hours4,
                global::Binance.Net.Enums.KlineInterval.SixHour => KlineInterval.Hours6,
                global::Binance.Net.Enums.KlineInterval.EightHour => KlineInterval.Hours8,
                global::Binance.Net.Enums.KlineInterval.TwelveHour => KlineInterval.Hours12,
                global::Binance.Net.Enums.KlineInterval.OneDay => KlineInterval.Day,
                global::Binance.Net.Enums.KlineInterval.OneWeek => KlineInterval.Week,
                _ => throw new NotImplementedException(),
            };
        }
    }
}