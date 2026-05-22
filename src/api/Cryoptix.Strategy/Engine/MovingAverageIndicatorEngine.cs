using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Cryoptix.Strategy.Engine
{
    public enum SmoothingType
    {
        Sma,
        Ema
    }

    public sealed class MovingAverageOptions
    {
        public SmoothingType Smoothing { get; init; } = SmoothingType.Sma;
    }

    public static class MovingAverageKeys
    {
        public const string SmaFast = "SMA_FAST";
        public const string SmaSlow = "SMA_SLOW";
        public const string EmaFast = "EMA_FAST";
        public const string EmaSlow = "EMA_SLOW";
    }
}

namespace Cryoptix.Strategy.Engine
{
    public sealed class MovingAverageIndicatorEngine(
        ILogger<MovingAverageIndicatorEngine> logger,
        MovingAverageOptions options) : IStrategyIndicatorEngine
    {
        private readonly ILogger<MovingAverageIndicatorEngine> _logger = logger;
        private readonly MovingAverageOptions _options = options;

        public Task<IndicatorComputationResult> ComputeAsync(
            StrategyAnalysisContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<Kline> klines = context.Klines;
            if (klines.Count == 0)
            {
                return Task.FromResult(IndicatorComputationResult.Empty(DateTime.UtcNow));
            }

            int fastPeriod = context.Strategy.FastPeriod;
            int slowPeriod = context.Strategy.SlowPeriod;

            var smoothing = _options.Smoothing;

            Dictionary<string, decimal> values = new();

            if (smoothing == SmoothingType.Sma)
            {
                decimal? fast = TryCalculateSmaRolling(klines, fastPeriod);
                decimal? slow = TryCalculateSmaRolling(klines, slowPeriod);

                if (fast.HasValue) values[MovingAverageKeys.SmaFast] = fast.Value;
                if (slow.HasValue) values[MovingAverageKeys.SmaSlow] = slow.Value;
            }
            else if (smoothing == SmoothingType.Ema)
            {
                decimal? fast = TryCalculateEma(klines, fastPeriod);
                decimal? slow = TryCalculateEma(klines, slowPeriod);

                if (fast.HasValue) values[MovingAverageKeys.EmaFast] = fast.Value;
                if (slow.HasValue) values[MovingAverageKeys.EmaSlow] = slow.Value;
            }

            _logger.LogDebug(
                "Computed indicators for {Symbol}. Values:{Values}",
                context.Strategy.Symbol,
                values);

            return Task.FromResult(new IndicatorComputationResult
            {
                TimestampUtc = DateTime.UtcNow,
                Values = values.ToImmutableDictionary()
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

            // Use simple EMA initialization: start with SMA of first period, then apply smoothing
            int start = klines.Count - period;
            decimal sma = 0m;
            for (int i = start; i < start + period; i++)
                sma += klines[i].Close;

            sma /= period;

            decimal multiplier = 2m / (period + 1);
            decimal ema = sma;

            for (int i = start + period; i < klines.Count; i++)
            {
                ema = ((klines[i].Close - ema) * multiplier) + ema;
            }

            return ema;
        }
    }
}
