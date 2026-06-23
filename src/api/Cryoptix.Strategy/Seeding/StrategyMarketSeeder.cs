using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Market.Extensions;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Seeding
{
    /// <summary>
    /// Represents the strategy market seeder.
    /// </summary>
    public sealed class StrategyMarketSeeder(
        ILogger<StrategyMarketSeeder> logger,
        IStrategyClock clock) : IStrategyMarketSeeder
    {
        private readonly ILogger<StrategyMarketSeeder> _logger = logger;
        private readonly IStrategyClock _clock = clock;

        /// <summary>
        /// Executes the seed async operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <param name="restApi">The rest api value.</param>
        /// <param name="klineWriter">The kline writer value.</param>
        /// <param name="cache">The cache value.</param>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The seed async result.</returns>
        public async Task SeedAsync(
            Strategies.Strategy strategy,
            IExchangeRestApi restApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            Cache.MarketDataCache cache,
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

            List<Symbol> symbols = await restApi.GetSymbolsAsync(cancellationToken);

            try
            {
                cache.SetSymbols(symbols);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache exchange symbols during seeding; continuing without cached symbols.");
            }

            DateTime endTime = _clock.UtcNow;
            DateTime startTime = GetSeedStartTime(strategy, endTime);

            LogInformation.FetchHistoricalKlines(
                _logger,
                strategy.Symbol,
                strategy.KlineInterval,
                startTime,
                endTime);

            List<Kline> historicalKlines = await restApi.GetKlinesAsync(
                symbol: strategy.Symbol,
                interval: strategy.KlineInterval,
                startTime: startTime,
                endTime: endTime,
                limit: strategy.KlineSeedLimit,
                cancellationToken: cancellationToken);

            foreach (Kline kline in historicalKlines.OrderBy(k => k.OpenTime))
            {
                await klineWriter.WriteAsync(
                    new KlineMarketEvent(kline, MarketEventSource.Seed),
                    cancellationToken);
            }

            LogInformation.SeededKlines(_logger, historicalKlines.Count, strategy.Symbol, strategy.KlineInterval);
        }

        private static DateTime GetSeedStartTime(Strategies.Strategy strategy, DateTime endTimeUtc)
        {
            int klineSeedSize
                = strategy.KlineSeedSize > 0 ? strategy.KlineSeedSize * strategy.KlineInterval.KlineIntervalToMinutes() : 1000;

            return endTimeUtc.AddMinutes(-klineSeedSize);
        }
    }
}
