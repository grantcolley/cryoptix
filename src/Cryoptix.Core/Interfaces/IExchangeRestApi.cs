using Cryoptix.Core.Enums;
using Cryoptix.Core.Models;

namespace Cryoptix.Core.Interfaces
{
    public interface IExchangeRestApi: IDisposable
    {
        Task<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default);
        Task<Order> PlaceOrderAsync(ClientOrder clientOrder, int? recWindow, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol, int? recWindow, CancellationToken cancellationToken = default);
        Task<string> CancelOrderAsync(string symbol, string orderId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Kline>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, int? limit, CancellationToken token = default);
        Task<OrderBook> GetOrderBookAsync(string symbol, int? limit, CancellationToken cancellationToken = default);
        Task<IEnumerable<Symbol>> GetSymbolsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Trade>> GetTradesAsync(string symbol, int? limit, CancellationToken cancellationToken);
    }
}
