using Cryoptix.Core.Enums;
using Cryoptix.Core.Interfaces;
using Cryoptix.Core.Models;

namespace Cryoptix.Exchange.Console.Test
{
    public class TestExchangeSubscriptionApi(IExchangeSubscriptionApi exchangeSubscriptionApi) : IExchangeSubscriptionApi
    {
        private readonly IExchangeSubscriptionApi _exchangeSubscriptionApi = exchangeSubscriptionApi;

        public Task<IAsyncDisposable> SubscribeToAccountUpdatesAsync(Credentials credentials, Action<AccountEventArgs> callback, Action<Exception> exception, CancellationToken cancellationToken)
        {
            static void wrappedCallback(AccountEventArgs args)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"SubscribeAccountInfoAsync({args?.Account?.Balances?.FirstOrDefault()?.Asset}) callback: {args?.Account?.Time} account:{args?.Account?.Name}");

                foreach (Balance balance in args?.Account?.Balances ?? [])
                {
                    System.Console.WriteLine($"asset: {balance.Asset} free:{balance.Free} locked:{balance.Locked}");
                }
            }

            System.Console.WriteLine($"SubscribeAccountInfoAsync({credentials.AccountName})");

            return _exchangeSubscriptionApi.SubscribeToAccountUpdatesAsync(credentials, wrappedCallback, OnException, cancellationToken);
        }

        public Task<IAsyncDisposable> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<KlineEventArgs> callback, Action<Exception> exception, CancellationToken cancellationToken)
        {
            static void wrappedCallback(KlineEventArgs args)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"SubscribeKlinesAsync({args.Klines?.FirstOrDefault()?.Symbol}) callback");

                foreach (Kline kline in args.Klines ?? [])
                {
                    System.Console.WriteLine($"{kline.Symbol} openTime:{kline.OpenTime:dd-MMM-yyyy HH:mm:ss} closeTime:{kline.CloseTime:dd-MMM-yyyy HH:mm:ss} open:{kline.Open} low:{kline.Low} high: {kline.High} close:{kline.Close} volume:{kline.Volume}");
                }   
            }

            System.Console.WriteLine($"SubscribeKlinesAsync({symbol}, {interval})");

            return _exchangeSubscriptionApi.SubscribeToKlineUpdatesAsync(symbol, interval, wrappedCallback, OnException, cancellationToken);
        }

        public Task<IAsyncDisposable> SubscribeToOrderBookAsync(string symbol, int? limit, Action<OrderBookEventArgs> callback, Action<Exception> exception, CancellationToken cancellationToken)
        {
            static void wrappedCallback(OrderBookEventArgs args)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"SubscribeOrderBook({args.OrderBook?.Symbol}) callback: {args.OrderBook?.Symbol} updateTime:{args.OrderBook?.UpdateTime} lastUpdateId: {args.OrderBook?.LastUpdateId}");

                if(args.OrderBook?.BestAsk != null)
                {
                    System.Console.WriteLine($"bestAsk price: {args.OrderBook?.BestAsk.Price} qty:{args.OrderBook?.BestAsk.Quantity}");
                }

                if(args.OrderBook?.BestBid != null)
                {
                    System.Console.WriteLine($"bestBid price: {args.OrderBook?.BestBid.Price} qty:{args.OrderBook?.BestBid.Quantity}");
                }

                if (args.OrderBook?.Asks != null)
                {
                    System.Console.WriteLine($"asks");
                    foreach (OrderBookPrice ask in args.OrderBook.Asks.OrderByDescending(a => a.Price))
                    {
                        System.Console.WriteLine($"price: {ask.Price} qty:{ask.Quantity}");
                    }
                }

                System.Console.WriteLine();

                if (args.OrderBook?.Bids != null)
                {
                    System.Console.WriteLine($"bids");
                    foreach (OrderBookPrice bid in args.OrderBook.Bids.OrderByDescending(b => b.Price))
                    {
                        System.Console.WriteLine($"price: {bid.Price} qty:{bid.Quantity}");
                    }
                }
            }

            System.Console.WriteLine($"SubscribeOrderBookAsync({symbol}, {limit})");

            return _exchangeSubscriptionApi.SubscribeToOrderBookAsync(symbol, limit, wrappedCallback, OnException, cancellationToken);
        }

        public Task<IAsyncDisposable> SubscribeToSymbolStatisticsAsync(IEnumerable<string> symbols, Action<StatisticsEventArgs> callback, Action<Exception> exception, CancellationToken cancellationToken)
        {
            static void wrappedCallback(StatisticsEventArgs args)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"SubscribeStatisticsAsync({args.Statistics?.FirstOrDefault()?.Symbol}) callback: {args.Statistics?.Count()} symbol statistics");

                foreach (SymbolStats stat in args.Statistics ?? [])
                {
                    System.Console.WriteLine($"{stat.Symbol} lastPrice:{stat.LastPrice} volume:{stat.Volume} priceChangePercent:{stat.PriceChangePercent} priceChange:{stat.PriceChange} weightedAvgPrice:{stat.WeightedAveragePrice}");
                }
            }

            System.Console.WriteLine($"SubscribeStatisticsAsync({string.Join(",", symbols)})");

            return _exchangeSubscriptionApi.SubscribeToSymbolStatisticsAsync(symbols, wrappedCallback, OnException, cancellationToken);
        }

        public Task<IAsyncDisposable> SubscribeToTradesAsync(string symbol, Action<TradeEventArgs> callback, Action<Exception> exception, CancellationToken cancellationToken)
        {
            static void wrappedCallback(TradeEventArgs args)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"SubscribeTradesAsync({args.Trades?.FirstOrDefault()?.Symbol}) callback: trades.Count:{args?.Trades?.Count()}");

                foreach (Trade trade in args?.Trades ?? [])
                {
                    System.Console.WriteLine($"{trade.Symbol} time:{trade.Time} price:{trade.Price} baseQuantity:{trade.BaseQuantity}");
                }
            }

            System.Console.WriteLine($"SubscribeTradesAsync({symbol})");

            return _exchangeSubscriptionApi.SubscribeToTradesAsync(symbol, wrappedCallback, OnException, cancellationToken);
        }

        private void OnException(Exception ex)
        {
            System.Console.WriteLine($"{ex}");
        }
    }
}
