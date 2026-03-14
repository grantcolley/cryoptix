using Cryoptix.Exchange.Models;

namespace Cryoptix.Exchange.Api
{
    public interface IExchangeRestApi: IDisposable
    {
        Exchanges.Exchange Exchange { get; }
        Task<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default);
        Task<Order> PlaceOrderAsync(ClientOrder clientOrder, int? recWindow, CancellationToken cancellationToken = default);
        Task<List<Order>> GetOpenOrdersAsync(string symbol, int? recWindow, CancellationToken cancellationToken = default);
        Task<string> CancelOrderAsync(string symbol, string orderId, CancellationToken cancellationToken = default);
        Task<List<Kline>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, int? limit, CancellationToken token = default);
        Task<OrderBook> GetOrderBookAsync(string symbol, int? limit, CancellationToken cancellationToken = default);
        Task<List<Symbol>> GetSymbolsAsync(CancellationToken cancellationToken);
        Task<List<Trade>> GetTradesAsync(string symbol, int? limit, CancellationToken cancellationToken);
    }
}
