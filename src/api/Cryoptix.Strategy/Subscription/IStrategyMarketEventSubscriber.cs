using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Subscription
{
    /// <summary>
    /// Defines the strategy market event subscriber contract.
    /// </summary>
    public interface IStrategyMarketEventSubscriber
    {
        /// <summary>
        /// Subscribes to the operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="subscriptionsApi">The subscriptions API.</param>
        /// <param name="channels">The channels.</param>
        /// <param name="orderBookRealtimeState">The order book realtime state.</param>
        /// <param name="accountRealtimeState">The account realtime state.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<StrategyMarketEventSubscriptions> SubscribeAsync(
            /// <summary>
            /// Gets the value.
            /// </summary>
            Strategies.Strategy strategy,
            /// <summary>
            /// Gets the value.
            /// </summary>
            Credentials? credentials,
            /// <summary>
            /// Gets the value.
            /// </summary>
            IExchangeSubscriptionApi subscriptionsApi,
            /// <summary>
            /// Gets the value.
            /// </summary>
            StrategyEventChannels channels,
            /// <summary>
            /// Gets the value.
            /// </summary>
            OrderBookRealtimeState orderBookRealtimeState,
            /// <summary>
            /// Gets the value.
            /// </summary>
            AccountRealtimeState accountRealtimeState,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken);
    }
}
