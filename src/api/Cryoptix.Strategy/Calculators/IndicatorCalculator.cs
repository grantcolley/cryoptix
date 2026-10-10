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
        /// <param name="period">The RSI period (must be > 0).</param>
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

        /// <summary>
        /// Initializes a <see cref="Macd"/> state from historical klines using the specified
        /// fast, slow, and signal periods.
        ///
        /// The fast and slow EMAs are seeded using simple moving averages and then processed
        /// sequentially using standard EMA smoothing. Once both EMAs are available, MACD
        /// values are calculated as the fast EMA minus the slow EMA. The signal line is
        /// seeded using the simple moving average of the first <paramref name="signalPeriod"/>
        /// MACD values and then processed using standard EMA smoothing for any remaining
        /// finalized history.
        /// </summary>
        /// <param name="initialKlines">
        /// A chronological list of klines containing sufficient finalized history.
        /// All klines are expected to be final except potentially the last one.
        /// </param>
        /// <param name="fastPeriod">The fast EMA period (must be &gt; 0).</param>
        /// <param name="slowPeriod">
        /// The slow EMA period (must be greater than <paramref name="fastPeriod"/>).
        /// </param>
        /// <param name="signalPeriod">The signal EMA period (must be &gt; 0).</param>
        /// <returns>
        /// A populated <see cref="Macd"/> representing the MACD state at the latest
        /// finalized kline, or <c>null</c> when the periods are invalid or insufficient
        /// finalized history is provided.
        /// </returns>
        public static Macd? MacdInitialize(IReadOnlyList<Kline> initialKlines, int fastPeriod, int slowPeriod, int signalPeriod)
        {
            if (fastPeriod <= 0
                || slowPeriod <= fastPeriod
                || signalPeriod <= 0)
            {
                return null;
            }

            // Cache invariant:
            // all klines are final except potentially the last one.
            // Build a usable history that excludes any trailing non-final kline.
            var usable = initialKlines.ToList();

            while (usable.Count > 0 && !usable[^1].Final)
                usable.RemoveAt(usable.Count - 1);

            // The first MACD value becomes available when the slow EMA is seeded.
            // We then need signalPeriod MACD values to seed the signal EMA.
            int requiredCount = slowPeriod + signalPeriod - 1;

            if (usable.Count < requiredCount)
                return null;

            // Seed the fast EMA using the first fastPeriod closes.
            decimal fastSum = 0m;

            for (int i = 0; i < fastPeriod; i++)
                fastSum += usable[i].Close;

            decimal fastEma = fastSum / fastPeriod;

            // Advance the fast EMA up to the point where the slow EMA becomes available.
            for (int i = fastPeriod; i < slowPeriod; i++)
                fastEma = CalculateEma(usable[i].Close, fastEma, fastPeriod);

            // Seed the slow EMA using the first slowPeriod closes.
            decimal slowSum = 0m;

            for (int i = 0; i < slowPeriod; i++)
                slowSum += usable[i].Close;

            decimal slowEma = slowSum / slowPeriod;

            // The first MACD value is available at slowPeriod - 1.
            decimal value = fastEma - slowEma;
            decimal signalSum = value;

            // Generate the remaining MACD values needed to seed the signal EMA.
            int signalSeedEnd = slowPeriod + signalPeriod - 1;

            for (int i = slowPeriod; i < signalSeedEnd; i++)
            {
                fastEma = CalculateEma(usable[i].Close, fastEma, fastPeriod);

                slowEma = CalculateEma(usable[i].Close, slowEma, slowPeriod);

                value = fastEma - slowEma;
                signalSum += value;
            }

            // Seed the signal EMA using the SMA of the first signalPeriod MACD values.
            decimal signal = signalSum / signalPeriod;

            // Process any remaining finalized history.
            for (int i = signalSeedEnd; i < usable.Count; i++)
            {
                fastEma = CalculateEma(usable[i].Close, fastEma, fastPeriod);

                slowEma = CalculateEma(usable[i].Close, slowEma, slowPeriod);

                value = fastEma - slowEma;

                signal = CalculateEma(value, signal, signalPeriod);
            }

            decimal histogram = value - signal;
            Kline latest = usable[^1];

            return new Macd
            {
                FastPeriod = fastPeriod,
                SlowPeriod = slowPeriod,
                SignalPeriod = signalPeriod,

                FastEma = fastEma,
                SlowEma = slowEma,

                Value = value,
                Signal = signal,
                Histogram = histogram,

                TimestampUtc = latest.CloseTime
            };
        }

        /// <summary>
        /// Calculates an updated <see cref="Macd"/> snapshot from an existing MACD state
        /// using the provided latest <paramref name="kline"/>.
        ///
        /// The method updates the fast and slow EMAs using the current kline close,
        /// calculates the MACD value as the fast EMA minus the slow EMA, updates the
        /// signal EMA from the new MACD value, and calculates the histogram as the
        /// MACD value minus the signal line.
        /// </summary>
        /// <param name="macd">
        /// The MACD state calculated from the most recent finalized kline.
        /// </param>
        /// <param name="kline">
        /// The current kline used to calculate the MACD snapshot.
        /// The kline may be a live or finalized kline.
        /// </param>
        /// <returns>
        /// A new <see cref="Macd"/> with updated EMA, MACD, signal, and histogram values,
        /// or the previous MACD if the provided kline is not newer than the existing
        /// MACD timestamp. The caller should persist the returned state only when
        /// <paramref name="kline"/> is final.
        /// </returns>
        public static Macd MacdUpdate(Macd macd, Kline kline)
        {
            if (kline.CloseTime <= macd.TimestampUtc)
                return macd;

            decimal fastEma = CalculateEma(kline.Close, macd.FastEma, macd.FastPeriod);

            decimal slowEma = CalculateEma(kline.Close, macd.SlowEma, macd.SlowPeriod);

            decimal value = fastEma - slowEma;

            decimal signal = CalculateEma(value, macd.Signal, macd.SignalPeriod);

            decimal histogram = value - signal;

            return new Macd
            {
                FastPeriod = macd.FastPeriod,
                SlowPeriod = macd.SlowPeriod,
                SignalPeriod = macd.SignalPeriod,

                FastEma = fastEma,
                SlowEma = slowEma,

                Value = value,
                Signal = signal,
                Histogram = histogram,

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
