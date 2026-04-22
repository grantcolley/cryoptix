using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Event;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Seeding
{
    public sealed class StrategyMarketSeeder(
        ILogger<StrategyMarketSeeder> logger,
        IStrategyClock clock) : IStrategyMarketSeeder
    {
        private readonly ILogger<StrategyMarketSeeder> _logger = logger;
        private readonly IStrategyClock _clock = clock;

        public async Task SeedAsync(
            Runtime.Strategy strategy,
            IExchangeRestApi restApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentNullException.ThrowIfNull(restApi);
            ArgumentNullException.ThrowIfNull(klineWriter);

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            if (strategy.KlineInterval == default)
                throw new InvalidOperationException("Strategy kline interval is required.");

            DateTime endTime = _clock.UtcNow;
            DateTime startTime = GetSeedStartTime(strategy, endTime);

            _logger.LogInformation(
                "Fetching historical klines for {Symbol} {Interval} from {Start:u} to {End:u}",
                strategy.Symbol,
                strategy.KlineInterval,
                startTime,
                endTime);

            List<Kline> historicalKlines = await restApi.GetKlinesAsync(
                symbol: strategy.Symbol,
                interval: strategy.KlineInterval,
                startTime: startTime,
                endTime: endTime,
                limit: null,
                cancellationToken: cancellationToken);

            foreach (Kline kline in historicalKlines.OrderBy(k => k.OpenTime))
            {
                await klineWriter.WriteAsync(
                    new KlineMarketEvent(kline, MarketEventSource.Seed),
                    cancellationToken);
            }

            _logger.LogInformation(
                "Seeded {Count} klines for {Symbol} {Interval}",
                historicalKlines.Count,
                strategy.Symbol,
                strategy.KlineInterval);
        }

        private static DateTime GetSeedStartTime(Runtime.Strategy strategy, DateTime endTimeUtc)
        {
            return endTimeUtc.AddDays(-2);
        }
    }
}
