using Cryoptix.Market.Data;
using Cryoptix.Strategy.Indicators;
using System.Linq;

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

        /// <summary>
        /// Initializes an <see cref="Rsi"/> state from historical klines for the specified <paramref name="period"/>.
        ///
        /// This method computes the initial average gains and losses using the first <c>period</c>
        /// changes, then runs the remaining history (if any) through Wilder smoothing so the
        /// returned <see cref="Rsi"/> contains properly smoothed averages ready for incremental updates.
        /// </summary>
        /// <param name="initialKlines">A chronological list of klines that includes at least <c>period + 1</c> entries.</param>
        /// <param name="period">The RSI period (must be &gt; 0).</param>
        /// <returns>
        /// A populated <see cref="Rsi"/> instance representing the RSI state at the latest kline,
        /// or <c>null</c> when <paramref name="period"/> is invalid or insufficient history is provided.
        /// </returns>
        public static Rsi? RsiInitialize(IReadOnlyList<Kline> initialKlines, int period)
        {
            if (period <= 0)
                return null;

            // Cache invariant:
            // all klines are final except potentially the last one.
            // Build a usable history that excludes any trailing non-final kline.
            var usable = initialKlines.ToList();
            while (usable.Count > 0 && !usable[^1].Final)
                usable.RemoveAt(usable.Count - 1);

            if (usable.Count < period + 1)
                return null;

            decimal gainSum = 0m;
            decimal lossSum = 0m;

            // Seed Wilder averages using the usable history.
            for (int i = 1; i <= period; i++)
            {
                decimal change = usable[i].Close - usable[i - 1].Close;

                decimal gain = change > 0m ? change : 0m;
                decimal loss = change < 0m ? -change : 0m;

                gainSum += gain;
                lossSum += loss;
            }

            decimal avgGain = gainSum / period;
            decimal avgLoss = lossSum / period;

            // Process remaining finalized history with Wilder smoothing.
            for (int i = period + 1; i < usable.Count; i++)
            {
                decimal change = usable[i].Close - usable[i - 1].Close;
                decimal gain = change > 0m ? change : 0m;
                decimal loss = change < 0m ? -change : 0m;

                avgGain = ((avgGain * (period - 1)) + gain) / period;
                avgLoss = ((avgLoss * (period - 1)) + loss) / period;
            }

            Kline latest = usable[^1];

            return new Rsi
            {
                Period = period,
                PreviousClose = latest.Close,
                AverageGain = avgGain,
                AverageLoss = avgLoss,
                Value = CalculateRsi(avgGain, avgLoss),
                TimestampUtc = latest.CloseTime
            };
        }

        /// <summary>
        /// Calculates an updated <see cref="Rsi"/> snapshot from an existing RSI state
        /// using the provided latest <paramref name="kline"/>.
        ///
        /// The method applies Wilder smoothing to the average gain and loss values and
        /// returns a new <see cref="Rsi"/> instance representing
        /// the RSI state at the provided kline's close time.
        /// </summary>
        /// <param name="rsi">The previous RSI state to use as the basis for the update.</param>
        /// <param name="kline">The latest finalized kline used to update the RSI.</param>
        /// <returns>
        /// A new <see cref="Rsi"/> with updated averages and value, or the previous RSI if the 
        /// provided kline is not finalized or is not newer than the existing RSI timestamp.
        /// </returns>
        public static Rsi RsiUpdate(Rsi rsi, Kline kline)
        {
            // Calculate RSI using the most recent close and Wilder smoothing

            if (!kline.Final || kline.CloseTime <= rsi.TimestampUtc)
                return rsi;

            decimal latest = kline.Close;
            decimal changeSincePrev = latest - rsi.PreviousClose;
            decimal gain = changeSincePrev > 0m ? changeSincePrev : 0m;
            decimal loss = changeSincePrev < 0m ? -changeSincePrev : 0m;

            decimal newAvgGain = ((rsi.AverageGain * (rsi.Period - 1)) + gain) / rsi.Period;
            decimal newAvgLoss = ((rsi.AverageLoss * (rsi.Period - 1)) + loss) / rsi.Period;

            return new Rsi
            {
                Period = rsi.Period,
                PreviousClose = latest,
                AverageGain = newAvgGain,
                AverageLoss = newAvgLoss,
                Value = CalculateRsi(newAvgGain, newAvgLoss),
                TimestampUtc = kline.CloseTime
            };
        }

        private static decimal CalculateRsi(decimal averageGain, decimal averageLoss)
        {
            if (averageGain == 0m && averageLoss == 0m)
                return 50m;

            if (averageLoss == 0m)
                return 100m;

            if (averageGain == 0m)
                return 0m;

            decimal rs = averageGain / averageLoss;

            return 100m - (100m / (1m + rs));
        }
    }
}
