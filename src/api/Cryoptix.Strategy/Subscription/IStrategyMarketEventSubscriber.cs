using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Subscription
{
    public interface IStrategyMarketEventSubscriber
    {
        Task<StrategyMarketEventSubscriptions> SubscribeAsync(
            Strategies.Strategy strategy,
            Credentials? credentials,
            IExchangeSubscriptionApi subscriptionsApi,
            StrategyEventChannels channels,
            OrderBookRealtimeState orderBookRealtimeState,
            AccountRealtimeState accountRealtimeState,
            CancellationToken cancellationToken);
    }
}
