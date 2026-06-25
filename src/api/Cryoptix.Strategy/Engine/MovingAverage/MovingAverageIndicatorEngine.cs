using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Cryoptix.Strategy.Engine.MovingAverage
{
    /// <summary>
    /// Represents the moving average indicator engine.
    /// </summary>
    public sealed class MovingAverageIndicatorEngine(ILogger<MovingAverageIndicatorEngine> logger) : IStrategyIndicatorEngine
    {
        private readonly ILogger<MovingAverageIndicatorEngine> _logger = logger;

        /// <summary>
        /// Executes the compute async operation.
        /// </summary>
        /// <param name="context">The context value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The compute async result.</returns>
        public Task<IndicatorComputationResult> ComputeAsync(
            StrategyAnalysisContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<Kline> klines = context.Klines;
            Kline? kline = context.CurrentEvent.Kline;

            if (context.CurrentEvent.Kind != MarketEventKind.Kline
                || kline == null
                || klines.Count == 0)
            {
                return Task.FromResult(IndicatorComputationResult.Empty(DateTime.MinValue));
            }

            Dictionary<string, decimal> values = [];

            if (context.Strategy.Periods != null)
            {
                DateTime currentCloseTime = kline.CloseTime;

                Indicators? previousIndicators = context.Indicators
                    .Where(i => i.TimestampUtc < currentCloseTime)
                    .OrderByDescending(i => i.TimestampUtc)
                    .FirstOrDefault();

                foreach (var kvp in context.Strategy.Periods)
                {
                    string name = kvp.Key ?? string.Empty;
                    Strategies.Period period = kvp.Value;

                    decimal? computed = null;

                    if (period.SmoothingType == MovingAverageSmoothingType.Sma)
                    {
                        computed = CalculateSma(klines, period.Value);
                    }
                    else if (period.SmoothingType == MovingAverageSmoothingType.Ema)
                    {
                         if (previousIndicators?.Values.TryGetValue(name, out decimal previousEma) == true)
                        {
                            computed = CalculateEma(klines, period.Value, previousEma);
                        }
                        else
                        {
                            // initialize EMA with SMA if no previous EMA exists
                            computed = CalculateSma(klines, period.Value);
                        }
                    }

                    if (computed.HasValue)
                    {
                        values[name] = computed.Value;
                    }
                }
            }

            LogDebug.IndicatorsComputed(_logger, context.Strategy.Symbol!, values);

            return Task.FromResult(new IndicatorComputationResult
            {
                Indicators = new Indicators
                {
                    TimestampUtc = kline.CloseTime,
                    Values = values.ToImmutableDictionary()
                },
            });
        }

        private static decimal? CalculateSma(IReadOnlyList<Kline> klines, int period)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            // SMA formula: SMA = SUM(close[1] + close[2] + ... + close[period]) / period

            decimal sum = 0m;
            int start = klines.Count - period;
            for (int i = start; i < klines.Count; i++)
                sum += klines[i].Close;

            return sum / period;
        }

        private static decimal? CalculateEma(IReadOnlyList<Kline> klines, int period, decimal previousEma)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            // EMA formula: newEMA = ((latestClose - previousEma) * multiplier) + previousEma

            decimal multiplier = 2m / (period + 1);

            decimal latestClose = klines[^1].Close;

            return (latestClose - previousEma) * multiplier + previousEma;
        }
    }
}
