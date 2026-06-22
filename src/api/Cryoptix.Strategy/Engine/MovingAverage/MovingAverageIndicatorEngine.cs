using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Cryoptix.Strategy.Engine.MovingAverage
{
    public sealed class MovingAverageIndicatorEngine(ILogger<MovingAverageIndicatorEngine> logger) : IStrategyIndicatorEngine
    {
        private readonly ILogger<MovingAverageIndicatorEngine> _logger = logger;

        public Task<IndicatorComputationResult> ComputeAsync(
            StrategyAnalysisContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<Kline> klines = context.Klines;
            if (context.CurrentEvent.Kind != MarketEventKind.Kline
                || klines.Count == 0)
            {
                return Task.FromResult(IndicatorComputationResult.Empty(DateTime.MinValue));
            }

            MovingAverageSmoothingType smoothingType = context.Strategy.SmoothingType;

            Dictionary<string, decimal> values = [];

            if (context.Strategy.Periods != null)
            {
                foreach (var kvp in context.Strategy.Periods)
                {
                    string name = kvp.Key ?? string.Empty;
                    int period = kvp.Value;

                    decimal? computed = null;
                    if (smoothingType == MovingAverageSmoothingType.Sma)
                    {
                        computed = TryCalculateSmaRolling(klines, period);
                    }
                    else if (smoothingType == MovingAverageSmoothingType.Ema)
                    {
                        computed = TryCalculateEma(klines, period);
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
                    TimestampUtc = context.CurrentEvent.Kline!.CloseTime,
                    Values = values.ToImmutableDictionary()
                },
            });
        }

        private static decimal? TryCalculateSmaRolling(IReadOnlyList<Kline> klines, int period)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            // Compute first window sum
            decimal sum = 0m;
            int start = klines.Count - period;
            for (int i = start; i < klines.Count; i++)
                sum += klines[i].Close;

            return sum / period;
        }

        private static decimal? TryCalculateEma(IReadOnlyList<Kline> klines, int period)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            // TODO: #11 Don't calculate EMA from the beginning of the cached klines every time a new candle arrives

            // EMA formula: EMA_today = (Price_today - EMA_yesterday) * multiplier + EMA_yesterday

            decimal multiplier = 2m / (period + 1);

            // Seed EMA using first period prices
            decimal ema = klines
                .Take(period)
                .Select(k => k.Close)
                .Average();

            // Compute EMA for the remaining prices
            for (int i = period; i < klines.Count; i++)
            {
                ema = ((klines[i].Close - ema) * multiplier) + ema;
            }

            return ema;
        }
    }
}
