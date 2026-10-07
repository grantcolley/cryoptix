using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Calculators;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Indicators;
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
        /// 
        /// NOTE: Indicator calculations have been extracted to
        /// Cryoptix.Strategy.Calculators.IndicatorCalculator to 
        /// keep the engine focused on orchestration and make 
        /// calculation helpers reusable/testable.
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

            List<Ema> emas = [];
            List<Rsi> rsis = [];
            Dictionary<string, decimal> series = [];
            Dictionary<string, decimal> snapshots = [];

            if (context.Strategy.Indicators != null)
            {
                DateTime currentCloseTime = kline.CloseTime;

                foreach (var kvp in context.Strategy.Indicators)
                {
                    Indicator indicator = kvp.Value;

                    if (indicator.IndicatorType == IndicatorType.Sma)
                    {
                        decimal? computed = IndicatorCalculator.Sma(klines, indicator.Value);

                        if (computed.HasValue)
                        {
                            series[kvp.Key] = computed.Value;
                        }
                    }
                    else if (indicator.IndicatorType == IndicatorType.Ema)
                    {
                        Ema? ema;
                        Ema? previousEma = context.Emas?.FirstOrDefault(x => x.Period == indicator.Value);

                        if (previousEma == null)
                        {
                            ema = IndicatorCalculator.EmaInitialize(klines, indicator.Value);
                        }
                        else
                        {
                            ema = IndicatorCalculator.EmaUpdate(previousEma, kline);
                        }

                        if (ema != null)
                        {
                            series[kvp.Key] = ema.Value;

                            if (kline.Final)
                            {
                                // Only add to the list of EMAs if the kline is final,
                                // to prevent caching live EMA calculation values and
                                // avoid duplicates in the next computation.
                                emas.Add(ema);
                            }
                        }
                    }
                    else if (indicator.IndicatorType == IndicatorType.Rsi)
                    {
                        Rsi? rsi = null;
                        Rsi? prev = context.Rsis?.FirstOrDefault(x => x.Period == indicator.Value);

                        if (prev == null)
                        {
                            rsi = IndicatorCalculator.RsiInitialize(klines, indicator.Value);
                        }
                        else
                        {
                            rsi = IndicatorCalculator.RsiUpdate(prev, kline);
                        }

                        if (rsi != null)
                        {
                            snapshots[kvp.Key] = rsi.Value;

                            if (kline.Final)
                            {
                                // Only add to the list of RSIs if the kline is final,
                                // to prevent caching live RSI calculation values and
                                // avoid duplicates in the next computation.
                                rsis.Add(rsi);
                            }
                        }
                    }
                }
            }

            LogDebug.IndicatorsComputed(_logger, context.Strategy.Symbol!, series);

            return Task.FromResult(new IndicatorComputationResult
            {
                Emas = emas,
                Rsis = rsis,
                Indicators = new Market.Strategy.Indicators
                {
                    TimestampUtc = kline.OpenTime,
                    Series = series.ToImmutableDictionary(),
                    Snapshots = snapshots.ToImmutableDictionary(),
                },
            });
        }
    }
}
