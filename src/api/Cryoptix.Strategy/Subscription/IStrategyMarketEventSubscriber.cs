using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Subscription
{
    public interface IStrategyMarketEventSubscriber
    {
        Task<StrategyMarketEventSubscriptions> SubscribeAsync(
            Runtime.Strategy strategy,
            IExchangeSubscriptionApi subscriptionsApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            ChannelWriter<TradeMarketEvent> tradeWriter,
            CancellationToken cancellationToken);
    }
}
