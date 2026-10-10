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
            List<Macd> macds = [];
            Dictionary<string, decimal> series = [];
            Dictionary<string, decimal> snapshots = [];

            if (context.Strategy.Indicators != null)
            {
                DateTime currentCloseTime = kline.CloseTime;

                foreach (Indicator indicator in context.Strategy.Indicators)
                {
                    if (indicator.IndicatorType == IndicatorType.Sma)
                    {
                        decimal? computed = IndicatorCalculator.Sma(klines, indicator.Values[0].Value);

                        if (computed.HasValue)
                        {
                            series[indicator.Name!] = computed.Value;
                        }
                    }
                    else if (indicator.IndicatorType == IndicatorType.Ema)
                    {
                        Ema? ema;
                        Ema? previousEma = context.Emas?.FirstOrDefault(x => x.Period == indicator.Values[0].Value);

                        if (previousEma == null)
                        {
                            ema = IndicatorCalculator.EmaInitialize(klines, indicator.Values[0].Value);
                        }
                        else
                        {
                            ema = IndicatorCalculator.EmaUpdate(previousEma, kline);
                        }

                        if (ema != null)
                        {
                            series[indicator.Name!] = ema.Value;

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
                        Rsi? prev = context.Rsis?.FirstOrDefault(x => x.Period == indicator.Values[0].Value);

                        if (prev == null)
                        {
                            rsi = IndicatorCalculator.RsiInitialize(klines, indicator.Values[0].Value);
                        }
                        else
                        {
                            rsi = IndicatorCalculator.RsiUpdate(prev, kline);
                        }

                        if (rsi != null)
                        {
                            snapshots[indicator.Name!] = rsi.Value;

                            if (kline.Final)
                            {
                                // Only add to the list of RSIs if the kline is final,
                                // to prevent caching live RSI calculation values and
                                // avoid duplicates in the next computation.
                                rsis.Add(rsi);
                            }
                        }
                    }
                    else if(indicator.IndicatorType == IndicatorType.Macd)
                    {
                        Macd? macd = null;
                        Macd? prev = context.Macds?.FirstOrDefault(x => x.FastPeriod == indicator.Values[0].Value
                            && x.SlowPeriod == indicator.Values[1].Value
                            && x.SignalPeriod == indicator.Values[2].Value);

                        if (prev == null)
                        {
                            macd = IndicatorCalculator.MacdInitialize(klines, indicator.Values[0].Value, indicator.Values[1].Value, indicator.Values[2].Value);
                        }
                        else
                        {
                            macd = IndicatorCalculator.MacdUpdate(prev, kline);
                        }
                        if (macd != null)
                        {
                            snapshots[indicator.Name!] = macd.Value;

                            if (kline.Final)
                            {
                                // Only add to the list of MACDs if the kline is final,
                                // to prevent caching live MACD calculation values and
                                // avoid duplicates in the next computation.
                                macds.Add(macd);
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
                Macds = macds,
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
