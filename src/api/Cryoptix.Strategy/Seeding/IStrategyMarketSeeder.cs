using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Seeding
{
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
