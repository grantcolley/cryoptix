using Cryoptix.Market.Data;
using Cryoptix.Strategy.Indicators;

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
        /// Initializes an <see cref="Ema"/> state from historical klines for the specified
        /// <paramref name="period"/>.
        ///
        /// The initial EMA is seeded using the simple moving average of the first
        /// <c>period</c> finalized klines. Any remaining finalized history is then
        /// processed sequentially using the standard EMA smoothing formula so the
        /// returned <see cref="Ema"/> represents the EMA at the latest finalized kline.
        /// </summary>
        /// <param name="initialKlines">
        /// A chronological list of klines that includes at least <paramref name="period"/>
        /// finalized entries. All klines are expected to be final except potentially
        /// the last one.
        /// </param>
        /// <param name="period">The EMA period (must be &gt; 0).</param>
        /// <returns>
        /// A populated <see cref="Ema"/> representing the EMA state at the latest
        /// finalized kline, or <c>null</c> when <paramref name="period"/> is invalid
        /// or insufficient finalized history is provided.
        /// </returns>
        public static Ema? EmaInitialize(IReadOnlyList<Kline> initialKlines, int period)
        {
            if (period <= 0)
                return null;

            // Cache invariant:
            // all klines are final except potentially the last one.
            // Build a usable history that excludes any trailing non-final kline.
            var usable = initialKlines.ToList();

            while (usable.Count > 0 && !usable[^1].Final)
                usable.RemoveAt(usable.Count - 1);

            if (usable.Count < period)
                return null;

            // Seed the EMA using the SMA of the first period closes.
            decimal sum = 0m;

            for (int i = 0; i < period; i++)
                sum += usable[i].Close;

            decimal value = sum / period;

            // Process remaining finalized history using EMA smoothing.
            for (int i = period; i < usable.Count; i++)
            {
                value = CalculateEma(usable[i].Close, value, period);
            }

            Kline latest = usable[^1];

            return new Ema
            {
                Period = period,
                Value = value,
                TimestampUtc = latest.CloseTime
            };
        }

        /// <summary>
        /// Calculates an updated <see cref="Ema"/> snapshot from an existing EMA state
        /// using the provided latest <paramref name="kline"/>.
        ///
        /// The method applies standard EMA smoothing to the current kline close and
        /// returns a new <see cref="Ema"/> representing the EMA at the provided
        /// kline's close time.
        /// </summary>
        /// <param name="ema">
        /// The EMA state calculated from the most recent finalized kline.
        /// </param>
        /// <param name="kline">
        /// The current kline used to calculate the EMA snapshot.
        /// The kline may be a live or finalized kline.
        /// </param>
        /// <returns>
        /// A new <see cref="Ema"/> with the updated value, or the previous EMA if the
        /// provided kline is not newer than the existing EMA timestamp.
        /// The caller should persist the returned state only when
        /// <paramref name="kline"/> is final.
        /// </returns>
        public static Ema EmaUpdate(Ema ema, Kline kline)
        {
            if (kline.CloseTime <= ema.TimestampUtc)
                return ema;

            decimal value = CalculateEma(kline.Close, ema.Value, ema.Period);

            return new Ema
            {
                Period = ema.Period,
                Value = value,
                TimestampUtc = kline.CloseTime
            };
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
        /// returns a new <see cref="Rsi"/> instance representing the RSI state at the 
        /// provided kline's close time.
        /// </summary>
        /// <param name="rsi">The RSI state calculated from the most recent finalized kline.</param>
        /// <param name="kline">
        /// The current kline used to calculate the RSI snapshot. 
        /// The kline may be a live or finalized kline.
        /// </param>
        /// <returns>
        /// A new <see cref="Rsi"/> with updated averages and value, or the previous RSI if the 
        /// provided kline is not newer than the existing RSI timestamp.
        /// The caller should persist the returned state only when <paramref name="kline"/> is final.
        /// </returns>
        public static Rsi RsiUpdate(Rsi rsi, Kline kline)
        {
            // Calculate RSI using the most recent close and Wilder smoothing

            if (kline.CloseTime <= rsi.TimestampUtc)
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

        private static decimal CalculateEma(decimal value, decimal previousEma, int period)
        {
            decimal multiplier = 2m / (period + 1);

            return (value - previousEma) * multiplier + previousEma;
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
