using Cryoptix.Market.Data;

namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Defines the i exchange rest api contract.
    /// </summary>
    public interface IExchangeRestApi : IDisposable
    {
        /// <summary>
        /// The exchange supported by this REST API implementation.
        /// </summary>
        Market.Data.Exchange Exchange { get; }
        /// <summary>
        /// Gets account information for the configured API credentials.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An <see cref="Account"/> containing balances and fee information.</returns>
        Task<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Places an order on the exchange.
        /// </summary>
        /// <param name="clientOrder">Order request details.</param>
        /// <param name="recWindow">Optional receive window for signed requests.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The placed <see cref="Order"/>.</returns>
        Task<Order> PlaceOrderAsync(ClientOrder clientOrder, int? recWindow, CancellationToken cancellationToken = default);
        Task<List<Order>> GetOpenOrdersAsync(string symbol, int? recWindow, CancellationToken cancellationToken = default);
        Task<string> CancelOrderAsync(string symbol, string orderId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieves historical klines (candles) for the requested symbol and interval in the given time range.
        /// Implementations may page results when the range exceeds service limits.
        /// </summary>
        /// <param name="symbol">Trading symbol (e.g. BTCUSDT).</param>
        /// <param name="interval">Kline interval.</param>
        /// <param name="startTime">Inclusive start time for klines.</param>
        /// <param name="endTime">Inclusive end time for klines.</param>
        /// <param name="limit">Maximum number of results to request per page; implementations may use paging.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>List of <see cref="Kline"/> objects covering the requested range.</returns>
        Task<List<Kline>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, int? limit, CancellationToken cancellationToken = default);
        Task<OrderBook> GetOrderBookAsync(string symbol, int? limit, CancellationToken cancellationToken = default);
        Task<List<Symbol>> GetSymbolsAsync(CancellationToken cancellationToken);
        Task<List<Trade>> GetTradesAsync(string symbol, int? limit, CancellationToken cancellationToken);
    }
}
