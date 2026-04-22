using Cryoptix.Market.Args;
using Cryoptix.Market.Data;

namespace Cryoptix.Exchange.Api
{
    public interface IExchangeSubscriptionApi
    {
        Market.Data.Exchange Exchange { get; }
        Task<IAsyncDisposable> SubscribeToAccountUpdatesAsync(Credentials user, Action<AccountEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<KlineEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToOrderBookAsync(string symbol, int? limit, Action<OrderBookEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToSymbolStatisticsAsync(IEnumerable<string> symbols, Action<StatisticsEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
        Task<IAsyncDisposable> SubscribeToTradesAsync(string symbol, Action<TradeEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken);
    }
}
