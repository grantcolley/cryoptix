using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Calculators
{
    /// <summary>
    /// Provides common indicator calculation helpers (SMA, EMA, etc.).
    /// </summary>
    public static class IndicatorCalculator
    {
        /// <summary>
        /// Calculates the simple moving average for the provided klines and period.
        /// Returns null when period is invalid or not enough data.
        /// </summary>
        public static decimal? Sma(IReadOnlyList<Kline> klines, int period)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            decimal sum = 0m;
            int start = klines.Count - period;
            for (int i = start; i < klines.Count; i++)
                sum += klines[i].Close;

            return sum / period;
        }

        /// <summary>
        /// Calculates the exponential moving average using the latest close and an optional previous EMA.
        /// If previousEma is null the EMA is initialized using the SMA. Returns null when period is invalid or not enough data.
        /// </summary>
        public static decimal? Ema(IReadOnlyList<Kline> klines, int period, decimal? previousEma = null)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            // When no previous EMA is provided, initialize EMA with SMA
            if (!previousEma.HasValue)
            {
                return Sma(klines, period);
            }

            decimal multiplier = 2m / (period + 1);
            decimal latestClose = klines[^1].Close;
            decimal prev = previousEma.Value;

            return (latestClose - prev) * multiplier + prev;
        }
    }
}
