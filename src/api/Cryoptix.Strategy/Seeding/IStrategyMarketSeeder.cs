using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Seeding
{
    /// <summary>
    /// Defines the i strategy market seeder contract.
    /// </summary>
    public interface IStrategyMarketSeeder
    {
        Task SeedAsync(
            Strategies.Strategy strategy,
            IExchangeRestApi restApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            Cache.MarketDataCache cache,
            CancellationToken cancellationToken);
    }
}
