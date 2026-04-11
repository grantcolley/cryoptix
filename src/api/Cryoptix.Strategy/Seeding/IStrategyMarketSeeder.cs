using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Seeding
{
    public interface IStrategyMarketSeeder
    {
        Task SeedAsync(
            Runtime.Strategy strategy,
            IExchangeRestApi restApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            CancellationToken cancellationToken);
    }
}
