using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Spot;
using Cryoptix.Core.Enums;
using Cryoptix.Core.Exceptions;
using Cryoptix.Core.Interfaces;
using Cryoptix.Core.Models;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using System.Data;
using System.Globalization;

namespace Cryoptix.Exchange.Binance
{
    public sealed class BinanceRestApi : IExchangeRestApi
    {
        private const Core.Enums.Exchange Exchange = Core.Enums.Exchange.Binance;
        private readonly IBinanceRestClient _binanceRestClient;
        private readonly string _accountName;
        private int _disposed;

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed != 0, nameof(BinanceRestApi));

        public BinanceRestApi(IBinanceRestClient binanceRestClient, Credentials credentials)
        {
            ArgumentNullException.ThrowIfNull(binanceRestClient);
            ArgumentNullException.ThrowIfNull(credentials);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(credentials.AccountName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(credentials.ApiKey);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(credentials.ApiSecret);

            _accountName = credentials.AccountName;
            _binanceRestClient = binanceRestClient;
            _binanceRestClient.SetApiCredentials(new ApiCredentials(credentials.ApiKey, credentials.ApiSecret));
        }

        public async Task<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            WebCallResult<BinanceAccountInfo> result = await _binanceRestClient.SpotApi.Account.GetAccountInfoAsync(ct: cancellationToken).ConfigureAwait(false);

            BinanceAccountInfo binanceAccountInfo = EnsureSuccess(result, "GetAccountInfoAsync()");

            Account accountInfo = new()
            {
                Name = _accountName,
                Exchange = Exchange,
                Time = binanceAccountInfo.UpdateTime,
                BuyerFee = binanceAccountInfo.BuyerFee,
                SellerFee = binanceAccountInfo.SellerFee
            };

            foreach (BinanceBalance balance in binanceAccountInfo.Balances)
            {
                accountInfo.Balances.Add(new Balance { Asset = balance.Asset, Free = balance.Available, Locked = balance.Locked });
            }

            return accountInfo;
        }

        public async Task<List<Kline>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, int? limit, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);

            if (startTime > endTime)
            { 
                throw new ArgumentException($"GetKlinesAsync startTime:{startTime} must be less than (or equal to) endTime:{endTime}", nameof(startTime));
            }

            global::Binance.Net.Enums.KlineInterval klineInterval = interval.ToKlineInterval();

            WebCallResult<IBinanceKline[]> result = await _binanceRestClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, klineInterval, startTime, endTime, limit, ct: cancellationToken).ConfigureAwait(false);

            IBinanceKline[] binanceKlines = EnsureSuccess(result, $"GetKlinesAsync({symbol}, {interval}, {startTime}, {endTime}, {limit})");

            List<Kline> klines = [.. from k in binanceKlines
                          select new Kline
                          {
                              Symbol = symbol,
                              Interval = interval,
                              Exchange = Exchange,
                              OpenTime = k.OpenTime,
                              CloseTime = k.CloseTime,
                              Open = k.OpenPrice,
                              High = k.HighPrice,
                              Low = k.LowPrice,
                              Close = k.ClosePrice,
                              Volume = k.Volume,
                              NumberOfTrades = k.TradeCount,
                              QuoteAssetVolume = k.QuoteVolume,
                              TakerBuyQuoteAssetVolume = k.TakerBuyQuoteVolume,
                              TakerBuyBaseAssetVolume = k.TakerBuyBaseVolume
                          }];

            return klines;
        }

        public async Task<List<Order>> GetOpenOrdersAsync(string? symbol, int? recWindow = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            WebCallResult<BinanceOrder[]> result = await _binanceRestClient.SpotApi.Trading.GetOpenOrdersAsync(symbol, recWindow, ct: cancellationToken).ConfigureAwait(false);

            BinanceOrder[] binanceOrders = EnsureSuccess(result, $"GetOpenOrdersAsync({symbol}, {recWindow})");

            List<Order> orders = [.. from o in binanceOrders
                          select new Order
                          {
                              AccountName = _accountName,
                              Exchange = Exchange,
                              Symbol = o.Symbol,
                              CreatedTime = o.CreateTime,
                              TransactTime = o.TransactTime,
                              UpdateTime = o.UpdateTime,
                              Id = Convert.ToString(o.Id, CultureInfo.InvariantCulture),
                              ClientOrderId = o.ClientOrderId,
                              Price = o.Price,
                              AverageFillPrice = o.AverageFillPrice,
                              StopPrice = o.StopPrice,
                              OriginalQuantity = o.Quantity,
                              QuantityFilled = o.QuantityFilled,
                              QuantityRemaining = o.QuantityRemaining,
                              TimeInForce = o.TimeInForce.ToCryoptixTimeInForce(),
                              Type = o.Type.ToCryoptixOrderType(),
                              Side = o.Side.ToCryoptixOrderSide(),
                              Status = o.Status.ToCryoptixOrderStatus(),
                              IsWorking = o.IsWorking
                          }];

            return orders;
        }

        public async Task<OrderBook> GetOrderBookAsync(string symbol, int? limit, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);

            WebCallResult<BinanceOrderBook> result = await _binanceRestClient.SpotApi.ExchangeData.GetOrderBookAsync(symbol, limit, ct: cancellationToken).ConfigureAwait(false);

            BinanceOrderBook binanceOrderBook = EnsureSuccess(result, $"GetOrderBookAsync({symbol}, {limit})");

            OrderBook orderBook = new()
            {
                Symbol = symbol,
                Exchange = Exchange,
                LastUpdateId = binanceOrderBook.LastUpdateId,
                Asks = [.. from ask in binanceOrderBook.Asks select new OrderBookPrice { Price = ask.Price, Quantity = ask.Quantity }],
                Bids = [.. from bid in binanceOrderBook.Bids select new OrderBookPrice { Price = bid.Price, Quantity = bid.Quantity }]
            };

            return orderBook;
        }

        public async Task<List<Symbol>> GetSymbolsAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            WebCallResult<BinanceExchangeInfo> result = await _binanceRestClient.SpotApi.ExchangeData.GetExchangeInfoAsync(ct: cancellationToken).ConfigureAwait(false);

            BinanceExchangeInfo binanceExchangeInfo = EnsureSuccess(result, "GetSymbolsAsync()");

            List<Symbol> symbols = [.. binanceExchangeInfo.Symbols.Select(s => s.ToCryoptixSymbol())];

            return symbols;
        }

        public async Task<List<Trade>> GetTradesAsync(string symbol, int? limit, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);

            WebCallResult<IBinanceRecentTrade[]> result = await _binanceRestClient.SpotApi.ExchangeData.GetRecentTradesAsync(symbol, limit, ct: cancellationToken).ConfigureAwait(false);

            IBinanceRecentTrade[] binanceRecentTrades = EnsureSuccess(result, $"GetTradesAsync({symbol}, {limit})");

            List<Trade> trades = [.. binanceRecentTrades.Select(t => new Trade
            {
                Symbol = symbol,
                Exchange = Exchange,
                Time = t.TradeTime,
                Id = t.OrderId,
                Price = t.Price,
                BaseQuantity = t.BaseQuantity,
                QuoteQuantity = t.QuoteQuantity
            })];

            return trades;
        }

        public async Task<Order> PlaceOrderAsync(ClientOrder clientOrder, int? recWindow, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(clientOrder);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(clientOrder.Symbol);

            if (clientOrder.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(clientOrder), clientOrder.Quantity, "clientOrder.Quantity must be greater than 0.");
            }

            if(clientOrder.Type == OrderType.Limit && (!clientOrder.Price.HasValue || clientOrder.Price <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(clientOrder), clientOrder.Price, "clientOrder.Price must be greater than 0 for Limit orders.");
            }

            if((clientOrder.Type == OrderType.StopLoss || clientOrder.Type == OrderType.StopLossLimit) && (!clientOrder.StopPrice.HasValue || clientOrder.StopPrice <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(clientOrder), clientOrder.StopPrice, "clientOrder.StopPrice must be greater than 0 for StopLoss/StopLossLimit orders.");
            }

            WebCallResult<BinancePlacedOrder> result = await _binanceRestClient.SpotApi.Trading.PlaceOrderAsync(
                clientOrder.Symbol,
                clientOrder.Side.ToBinanceOrderSide(),
                clientOrder.Type.ToSpotOrderType(),
                clientOrder.Quantity,
                price: clientOrder.Price,
                timeInForce: clientOrder.TimeInForce.ToBinanceTimeInForce(),
                stopPrice: clientOrder.StopPrice,
                receiveWindow: recWindow,
                ct: cancellationToken).ConfigureAwait(false);

            BinancePlacedOrder binancePlacedOrder = EnsureSuccess(result, $"PlaceOrderAsync([{clientOrder.Symbol},{clientOrder.Side.ToBinanceOrderSide()},{clientOrder.Type.ToSpotOrderType()},{clientOrder.Quantity},{clientOrder.Price},{clientOrder.TimeInForce.ToBinanceTimeInForce()},{clientOrder.StopPrice}], {recWindow})");

            Order order = new()
            {
                AccountName = _accountName,
                Exchange = Exchange,
                Symbol = binancePlacedOrder.Symbol,
                CreatedTime = binancePlacedOrder.CreateTime,
                TransactTime = binancePlacedOrder.TransactTime,
                UpdateTime = binancePlacedOrder.UpdateTime,
                Id = Convert.ToString(binancePlacedOrder.Id, CultureInfo.InvariantCulture),
                ClientOrderId = binancePlacedOrder.ClientOrderId,
                Price = binancePlacedOrder.Price,
                StopPrice = binancePlacedOrder.StopPrice,
                OriginalQuantity = binancePlacedOrder.Quantity,
                QuantityFilled = binancePlacedOrder.QuantityFilled,
                QuantityRemaining = binancePlacedOrder.QuantityRemaining,
                TimeInForce = binancePlacedOrder.TimeInForce.ToCryoptixTimeInForce(),
                Type = binancePlacedOrder.Type.ToCryoptixOrderType(),
                Side = binancePlacedOrder.Side.ToCryoptixOrderSide(),
                Status = binancePlacedOrder.Status.ToCryoptixOrderStatus(),
                IsWorking = binancePlacedOrder.IsWorking
            };

            return order;
        }

        public async Task<string> CancelOrderAsync(string symbol, string orderId, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(orderId);

            long binanceOrderId;

            try
            {
                binanceOrderId = Convert.ToInt64(orderId, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                throw new ArgumentException($"CancelOrderAsync invalid orderId:{orderId}", nameof(orderId), ex);
            }

            WebCallResult<BinanceOrderBase> result = await _binanceRestClient.SpotApi.Trading.CancelOrderAsync(symbol, binanceOrderId, ct: cancellationToken).ConfigureAwait(false);

            BinanceOrderBase binanceOrderBase = EnsureSuccess(result, $"CancelOrderAsync({symbol}, {orderId})");

            return binanceOrderBase.ClientOrderId;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Atomically set _disposed to 1; if it was already 1, another thread already disposed.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing)
            {
                _binanceRestClient.Dispose();
            }
        }

        private static T EnsureSuccess<T>(WebCallResult<T> result, string message)
        {
            if (!result.Success)
            {
                throw new ExchangeApiException(
                    message: $"{message}: {result.Error?.ToExchangeErrorMessage()}",
                    exchange: Exchange.ToString(),
                    inner: result.Error?.Exception);
            }

            if (result.Data == null)
            {
                throw new ExchangeApiException($"{message}: success but Data was null", exchange: Exchange.ToString());
            }

            return result.Data;
        }
    }
}
