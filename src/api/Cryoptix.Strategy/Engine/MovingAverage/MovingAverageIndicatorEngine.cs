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

            Dictionary<string, decimal> values = [];

            if (context.Strategy.Indicators != null)
            {
                DateTime currentCloseTime = kline.CloseTime;

                Market.Strategy.Indicators? previousIndicators = context.Indicators
                    .Where(i => i.TimestampUtc < currentCloseTime)
                    .OrderByDescending(i => i.TimestampUtc)
                    .FirstOrDefault();

                foreach (var kvp in context.Strategy.Indicators)
                {
                    string name = kvp.Key ?? string.Empty;
                    Indicator indicator = kvp.Value;

                    decimal? computed = null;

                    if (indicator.IndicatorType == IndicatorType.Sma)
                    {
                        computed = IndicatorCalculator.Sma(klines, indicator.Value);
                    }
                    else if (indicator.IndicatorType == IndicatorType.Ema)
                    {
                        if (previousIndicators?.Values.TryGetValue(name, out decimal previousEma) == true)
                        {
                            computed = IndicatorCalculator.Ema(klines, indicator.Value, previousEma);
                        }
                        else
                        {
                            computed = IndicatorCalculator.Ema(klines, indicator.Value);
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
                Indicators = new Market.Strategy.Indicators
                {
                    TimestampUtc = kline.CloseTime,
                    Values = values.ToImmutableDictionary()
                },
            });
        }
    }
}
