using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Snapshot;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Subscription
{
    public interface IStrategyMarketEventSubscriber
    {
        Task<StrategyMarketEventSubscriptions> SubscribeAsync(
            Runtime.Strategy strategy,
            Credentials? credentials,
            IExchangeSubscriptionApi subscriptionsApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            ChannelWriter<TradeMarketEvent> tradeWriter,
            OrderBookRealtimeState orderBookRealtimeState,
            AccountRealtimeState accountRealtimeState,
            CancellationToken cancellationToken);
    }
}
