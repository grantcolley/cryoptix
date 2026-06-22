using Cryoptix.Market.Args;
using Cryoptix.Market.Data;

namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Defines the i exchange subscription api contract.
    /// </summary>
    public interface IExchangeSubscriptionApi
    {
        /// <summary>
        /// The exchange supported by this subscription API implementation.
        /// </summary>
        Market.Data.Exchange Exchange { get; }
        /// <summary>
        /// Subscribes to account updates for the provided user credentials.
        /// The returned <see cref="IAsyncDisposable"/> must be disposed to stop the subscription.
        /// </summary>
        /// <param name="user">User credentials for the subscription.</param>
        /// <param name="onCallback">Callback invoked for each account event.</param>
        /// <param name="onError">Callback invoked when an error occurs.</param>
        /// <param name="cancellationToken">Cancellation token for the subscription call.</param>
        /// <returns>An async disposable representing the active subscription.</returns>
        Task<IAsyncDisposable> SubscribeToAccountUpdatesAsync(Credentials user, Action<AccountEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<KlineEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToOrderBookAsync(string symbol, int? limit, Action<OrderBookEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToSymbolStatisticsAsync(IEnumerable<string> symbols, Action<StatisticsEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToTradesAsync(string symbol, Action<TradeEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
    }
}
