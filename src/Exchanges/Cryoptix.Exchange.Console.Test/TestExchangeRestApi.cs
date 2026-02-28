using Cryoptix.Core.Enums;
using Cryoptix.Core.Interfaces;
using Cryoptix.Core.Models;

namespace Cryoptix.Exchange.Console.Test
{
    public class TestExchangeRestApi(IExchangeRestApi exchangeRestApi, string accountName) : IExchangeRestApi
    {
        private readonly IExchangeRestApi _exchangeRestApi = exchangeRestApi;
        private readonly string _accountName = accountName;
        private bool disposedValue;

        public async Task<string> CancelOrderAsync(string symbol, string orderId, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;

            try
            {
                System.Console.WriteLine($"CancelOrderAsync(symbol:{symbol} orderId:{orderId})");

                ArgumentNullException.ThrowIfNull(symbol, nameof(symbol));
                ArgumentNullException.ThrowIfNull(orderId, nameof(orderId));

                result = await _exchangeRestApi.CancelOrderAsync(symbol, orderId, cancellationToken);

                System.Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return result;
        }

        public async Task<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default)
        {
            Account account = new();

            try
            {
                account = await _exchangeRestApi.GetAccountInfoAsync(cancellationToken);

                System.Console.WriteLine($"GetAccountInfoAsync({_accountName})");

                System.Console.WriteLine($"time: {account?.Time} user:{account?.Name}");

                foreach (Balance balance in account?.Balances ?? [])
                {
                    System.Console.WriteLine($"asset: {balance.Asset} free:{balance.Free} locked:{balance.Locked}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return account ?? throw new NullReferenceException(nameof(account));
        }

        public async Task<List<Kline>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, int? limit, CancellationToken token = default)
        {
            List<Kline> klines = [];

            try
            {
                klines = await _exchangeRestApi.GetKlinesAsync(symbol, KlineInterval.Minute, startTime, endTime,  limit, token);

                System.Console.WriteLine($"GetKlinesAsync({symbol}, {interval}, {startTime}, {endTime}, {limit})");

                int i = 1;
                foreach (Kline kline in klines)
                {
                    System.Console.WriteLine($"{i} {kline.Symbol} openTime:{kline.OpenTime:dd-MMM-yyyy HH:mm:ss} closeTime:{kline.CloseTime:dd-MMM-yyyy HH:mm:ss} open:{kline.Open} low:{kline.Low} high: {kline.High} close:{kline.Close} volume:{kline.Volume}");
                    i++;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return klines;
        }

        public async Task<List<Order>> GetOpenOrdersAsync(string symbol, int? recWindow, CancellationToken cancellationToken = default)
        {
            List<Order> orders = [];

            try
            {
                ArgumentNullException.ThrowIfNull(symbol, nameof(symbol));

                orders = await _exchangeRestApi.GetOpenOrdersAsync(symbol, recWindow, cancellationToken);

                System.Console.WriteLine($"GetOpenOrdersAsync({_accountName}, {symbol}, {recWindow})");

                int i = 1;

                foreach (Order order in orders)
                {
                    System.Console.WriteLine($"{i} orderId: {order.Id} clientOrderId: {order.ClientOrderId} price: {order.Price} originalQty: {order.OriginalQuantity}                     System.Console.WriteLine($\"{{i}} orderId: {{order.Id}} clientOrderId: {{order.ClientOrderId}} price: {{order.Price}} originalQty: {{order.OriginalQuantity}} executedQty: {{order.QuantityFilled}} status: {{order.Status}} timeInForce: {{order.TimeInForce}} type: {{order.Type}} side: {{order.Side}} stopPrice: {{order.StopPrice}} time: {{order.CreatedTime}} isWorking: {{order.IsWorking}}\");\r\n: {order.QuantityFilled} status: {order.Status} timeInForce: {order.TimeInForce} type: {order.Type} side: {order.Side} stopPrice: {order.StopPrice} time: {order.CreatedTime} isWorking: {order.IsWorking}");
                    i++;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return orders;
        }

        public async Task<OrderBook> GetOrderBookAsync(string symbol, int? limit, CancellationToken cancellationToken = default)
        {
            OrderBook? orderBook = null;

            try
            {
                ArgumentNullException.ThrowIfNull(symbol, nameof(symbol));

                orderBook = await _exchangeRestApi.GetOrderBookAsync(symbol, limit, cancellationToken);

                System.Console.WriteLine($"GetOrderBookAsync({symbol}, {limit})");

                System.Console.WriteLine($"{orderBook.Symbol} lastUpdateId: {orderBook.LastUpdateId}");

                if (orderBook.Asks != null)
                {
                    System.Console.WriteLine($"asks");
                    foreach (OrderBookPrice ask in orderBook.Asks.OrderByDescending(a => a.Price))
                    {
                        System.Console.WriteLine($"price: {ask.Price} qty:{ask.Quantity}");
                    }
                }

                System.Console.WriteLine();

                if (orderBook.Bids != null)
                {
                    System.Console.WriteLine($"bids");
                    foreach (OrderBookPrice bid in orderBook.Bids.OrderByDescending(b => b.Price))
                    {
                        System.Console.WriteLine($"price: {bid.Price} qty:{bid.Quantity}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return orderBook ?? new OrderBook();
        }

        public async Task<List<Symbol>> GetSymbolsAsync(CancellationToken cancellationToken)
        {
            List<Symbol> symbols = [];

            try
            {
                symbols = await _exchangeRestApi.GetSymbolsAsync(cancellationToken);

                System.Console.WriteLine($"GetSymbolsAsync()");

                int i = 1;
                foreach (Symbol symbol in symbols)
                {
                    System.Console.WriteLine($"{i} {symbol.Name} baseAsset:{symbol.BaseAsset?.Symbol} quoteAsset:{symbol.QuoteAsset?.Symbol} notionalMinValue: { symbol.NotionalMinimumValue} TickSize:{symbol.TickSize} lotSizeMinimum:{symbol.LotSize}");
                    i++;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return symbols;
        }

        public async Task<List<Trade>> GetTradesAsync(string symbol, int? limit, CancellationToken cancellationToken)
        {
            List<Trade> trades = [];

            try
            {
                trades = await _exchangeRestApi.GetTradesAsync(symbol, limit, cancellationToken);

                System.Console.WriteLine($"GetTradesAsync({symbol}, {limit})");

                int i = 1;
                foreach (Trade trade in trades)
                {
                    System.Console.WriteLine($"{i} {trade.Time:dd-MMM-yyyy HH:mm:ss} {trade.Symbol} price:{trade.Price} quoteQty:{trade.QuoteQuantity}");
                    i++;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return trades;
        }

        public async Task<Order> PlaceOrderAsync(ClientOrder clientOrder, int? recWindow, CancellationToken cancellationToken = default)
        {
            Order order = new();

            try
            {
                ArgumentNullException.ThrowIfNull(clientOrder, nameof(clientOrder));

                System.Console.WriteLine($"PlaceOrder({_accountName}, {clientOrder.Symbol}, {clientOrder.Side}, {clientOrder.Type}, {clientOrder.Quantity}, {clientOrder.Price})");

                order = await _exchangeRestApi.PlaceOrderAsync(clientOrder, recWindow, cancellationToken);

                System.Console.WriteLine($"orderId: {order.Id} clientOrderId: {order.ClientOrderId} price: {order.Price} originalQty: {order.OriginalQuantity} qtyFilled: {order.QuantityFilled} status: {order.Status} timeInForce: {order.TimeInForce} type: {order.Type} side: {order.Side} stopPrice: {order.StopPrice} transactTime: {order.TransactTime} isWorking: {order.IsWorking}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            finally
            {
                System.Console.WriteLine();
            }

            return order;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _exchangeRestApi.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TestExchangeRestApi()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
