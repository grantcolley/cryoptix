using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Analysis;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Engine
{
    public sealed class MovingAverageIndicatorEngine(
        ILogger<MovingAverageIndicatorEngine> logger) : IStrategyIndicatorEngine
    {
        private readonly ILogger<MovingAverageIndicatorEngine> _logger = logger;

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

            decimal? fastSma = TryCalculateSma(klines, fastPeriod);
            decimal? slowSma = TryCalculateSma(klines, slowPeriod);

            Dictionary<string, decimal> values = new();

            if (fastSma.HasValue)
                values["SMA_FAST"] = fastSma.Value;

            if (slowSma.HasValue)
                values["SMA_SLOW"] = slowSma.Value;

            _logger.LogDebug(
                "Computed indicators for {Symbol}. Fast:{Fast} Slow:{Slow}",
                context.Strategy.Symbol,
                fastSma,
                slowSma);

            return Task.FromResult(new IndicatorComputationResult
            {
                TimestampUtc = DateTime.UtcNow,
                Values = values
            });
        }

        private static decimal? TryCalculateSma(IReadOnlyList<Kline> klines, int period)
        {
            if (period <= 0 || klines.Count < period)
                return null;

            decimal sum = 0m;
            for (int i = klines.Count - period; i < klines.Count; i++)
            {
                sum += klines[i].Close;
            }

            return sum / period;
        }
    }
}
