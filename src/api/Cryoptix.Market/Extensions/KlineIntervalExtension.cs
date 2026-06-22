using Cryoptix.Market.Data;

namespace Cryoptix.Market.Extensions
{
    /// <summary>
    /// Represents the kline interval extension.
    /// </summary>
    public static class KlineIntervalExtension
    {
        /// <summary>
        /// Executes the kline interval to minutes operation.
        /// </summary>
        /// <param name="interval">The interval value.</param>
        /// <returns>The kline interval to minutes result.</returns>
        public static int KlineIntervalToMinutes(this KlineInterval interval)
        {
            return interval switch
            {
                KlineInterval.Minute => 1,
                KlineInterval.Minutes3 => 3,
                KlineInterval.Minutes5 => 5,
                KlineInterval.Minutes15 => 15,
                KlineInterval.Minutes30 => 30,
                KlineInterval.Hour => 60,
                KlineInterval.Hours2 => 120,
                KlineInterval.Hours4 => 240,
                KlineInterval.Hours6 => 360,
                KlineInterval.Hours8 => 480,
                KlineInterval.Hours12 => 720,
                KlineInterval.Day => 1440,
                KlineInterval.Days3 => 4320,
                KlineInterval.Week => 10080,
                KlineInterval.Month => 43200,
                _ => throw new ArgumentOutOfRangeException(nameof(interval), $"Not expected interval value: {interval}"),
            };
        }
    }
}
