using Cryoptix.Core.Enums;
using Cryoptix.Core.Models;

namespace Cryoptix.Core.Interfaces
{
    public interface IExchangeSubscriptionApi
    {
        Task<IAsyncDisposable> SubscribeToAccountUpdatesAsync(Credentials user, Action<AccountEventArgs> callback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<KlineEventArgs> callback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToOrderBookAsync(string symbol, int? limit, Action<OrderBookEventArgs> callback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToSymbolStatisticsAsync(IEnumerable<string> symbols, Action<StatisticsEventArgs> callback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToTradesAsync(string symbol, Action<TradeEventArgs> callback, Action<Exception> onError, CancellationToken cancellationToken);
    }
}
