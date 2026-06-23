using Cryoptix.Market.Args;
using Cryoptix.Market.Data;

namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Defines the exchange subscription api contract.
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
        /// <summary>
        /// Subscribes to the kline updates.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="onCallback">The on callback.</param>
        /// <param name="onError">The on error.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<IAsyncDisposable> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<KlineEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        /// <summary>
        /// Subscribes to the order book.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="onCallback">The on callback.</param>
        /// <param name="onError">The on error.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<IAsyncDisposable> SubscribeToOrderBookAsync(string symbol, int? limit, Action<OrderBookEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        /// <summary>
        /// Subscribes to the symbol statistics.
        /// </summary>
        /// <param name="symbols">The symbols.</param>
        /// <param name="onCallback">The on callback.</param>
        /// <param name="onError">The on error.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<IAsyncDisposable> SubscribeToSymbolStatisticsAsync(IEnumerable<string> symbols, Action<StatisticsEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        /// <summary>
        /// Subscribes to the trades.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="onCallback">The on callback.</param>
        /// <param name="onError">The on error.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<IAsyncDisposable> SubscribeToTradesAsync(string symbol, Action<TradeEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
    }
}
