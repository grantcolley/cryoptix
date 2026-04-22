using Cryoptix.Exchange.Api;
using Cryoptix.Market.Models;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.Snapshot;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Subscription
{
    using Cryoptix.Market.Args;
    using System.Threading.Channels;

    public sealed class StrategyMarketEventSubscriber(
        ILogger<StrategyMarketEventSubscriber> logger) : IStrategyMarketEventSubscriber
    {
        private readonly ILogger<StrategyMarketEventSubscriber> _logger = logger;

        public async Task<StrategyMarketEventSubscriptions> SubscribeAsync(
            Runtime.Strategy strategy,
            Credentials? credentials,
            IExchangeSubscriptionApi subscriptionsApi,
            ChannelWriter<KlineMarketEvent> klineWriter,
            ChannelWriter<TradeMarketEvent> tradeWriter,
            OrderBookRealtimeState orderBookRealtimeState,
            AccountRealtimeState accountRealtimeState,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentNullException.ThrowIfNull(subscriptionsApi);
            ArgumentNullException.ThrowIfNull(klineWriter);
            ArgumentNullException.ThrowIfNull(tradeWriter);
            ArgumentNullException.ThrowIfNull(orderBookRealtimeState);
            ArgumentNullException.ThrowIfNull(accountRealtimeState);

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            CancellationTokenSource sessionCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            IAsyncDisposable? klineSubscription = null;
            IAsyncDisposable? tradeSubscription = null;
            IAsyncDisposable? orderBookSubscription = null;
            IAsyncDisposable? accountSubscription = null;

            try
            {
                klineSubscription = await subscriptionsApi.SubscribeToKlineUpdatesAsync(
                    symbol: strategy.Symbol,
                    interval: strategy.KlineInterval,
                    onCallback: args => OnKlineCallback(strategy, klineWriter, args),
                    onError: ex => OnKlineError(strategy, ex),
                    cancellationToken: sessionCancellationTokenSource.Token);

                tradeSubscription = await subscriptionsApi.SubscribeToTradesAsync(
                    symbol: strategy.Symbol,
                    onCallback: args => OnTradeCallback(strategy, tradeWriter, args),
                    onError: ex => OnTradeError(strategy, ex),
                    cancellationToken: sessionCancellationTokenSource.Token);

                orderBookSubscription = await subscriptionsApi.SubscribeToOrderBookAsync(
                    symbol: strategy.Symbol,
                    limit: strategy.OrderBookLimit,
                    onCallback: args => OnOrderBookCallback(strategy, orderBookRealtimeState, args),
                    onError: ex => OnOrderBookError(strategy, ex),
                    cancellationToken: sessionCancellationTokenSource.Token);

                if (credentials != null)
                {
                    accountSubscription = await subscriptionsApi.SubscribeToAccountUpdatesAsync(
                        user: credentials,
                        onCallback: args => OnAccountCallback(accountRealtimeState, args),
                        onError: ex => OnAccountError(credentials, ex),
                        cancellationToken: sessionCancellationTokenSource.Token);
                }
                else
                {
                    _logger.LogWarning(
                        "No credentials supplied for strategy {Symbol}; account updates will not be subscribed.",
                        strategy.Symbol);
                }

                CompositeAsyncDisposable compositeHandle = accountSubscription == null
                    ? new CompositeAsyncDisposable(klineSubscription, tradeSubscription, orderBookSubscription)
                    : new CompositeAsyncDisposable(klineSubscription, tradeSubscription, orderBookSubscription, accountSubscription);

                Task completionTask = WaitUntilCancelledCleanlyAsync(sessionCancellationTokenSource.Token);

                _logger.LogInformation(
                    "Started subscriptions for {Symbol} {Interval}, including order book{AccountSuffix}",
                    strategy.Symbol,
                    strategy.KlineInterval,
                    accountSubscription == null ? "" : " and account");

                return new StrategyMarketEventSubscriptions(
                    compositeHandle,
                    sessionCancellationTokenSource,
                    completionTask);
            }
            catch
            {
                if (accountSubscription != null)
                {
                    try { await accountSubscription.DisposeAsync(); } catch { }
                }

                if (orderBookSubscription != null)
                {
                    try { await orderBookSubscription.DisposeAsync(); } catch { }
                }

                if (tradeSubscription != null)
                {
                    try { await tradeSubscription.DisposeAsync(); } catch { }
                }

                if (klineSubscription != null)
                {
                    try { await klineSubscription.DisposeAsync(); } catch { }
                }

                sessionCancellationTokenSource.Dispose();
                throw;
            }
        }

        private void OnKlineCallback(
            Runtime.Strategy strategy,
            ChannelWriter<KlineMarketEvent> writer,
            KlineEventArgs args)
        {
            if (args.Klines == null || !args.Klines.Any())
            {
                _logger.LogWarning(
                    "Received kline update with no klines for {Symbol} {Interval}",
                    strategy.Symbol,
                    strategy.KlineInterval);
                return;
            }

            foreach (Kline kline in args.Klines)
            {
                if (!writer.TryWrite(new KlineMarketEvent(kline, MarketEventSource.Live)))
                {
                    _logger.LogWarning(
                        "Failed to enqueue live kline for {Symbol} {Interval}. Channel may be full or closed.",
                        kline.Symbol,
                        kline.Interval);
                }
            }
        }

        private void OnTradeCallback(
            Runtime.Strategy strategy,
            ChannelWriter<TradeMarketEvent> writer,
            TradeEventArgs args)
        {
            if (args.Trades == null || !args.Trades.Any())
            {
                _logger.LogWarning(
                    "Received trade update with no trades for {Symbol}",
                    strategy.Symbol);
                return;
            }

            int dropped = 0;

            foreach (Trade trade in args.Trades)
            {
                if (!writer.TryWrite(new TradeMarketEvent(trade)))
                {
                    dropped++;
                }
            }

            if (dropped > 0)
            {
                _logger.LogWarning(
                    "Dropped {DroppedCount} trade events for {Symbol} due to channel pressure or closure.",
                    dropped,
                    strategy.Symbol);
            }
        }

        private void OnOrderBookCallback(
            Runtime.Strategy strategy,
            OrderBookRealtimeState orderBookRealtimeState,
            OrderBookEventArgs args)
        {
            if (args.OrderBook == null)
            {
                _logger.LogWarning(
                    "Received order book update with null payload for {Symbol}",
                    strategy.Symbol);
                return;
            }

            orderBookRealtimeState.Update(args.OrderBook);

            _logger.LogInformation(
                "ORDER BOOK {Symbol} UpdateTime:{UpdateTime:u} BestAsk.Price:{BestAskPrice} BestAsk.Quantity:{BestAskQuantity} BestBid.Price:{BestBidPrice} BestBid.Quantity:{BestBidQuantity}",
                args.OrderBook.Symbol,
                args.OrderBook.UpdateTime,
                args.OrderBook.BestAsk?.Price,
                args.OrderBook.BestAsk?.Quantity,
                args.OrderBook.BestBid?.Price,
                args.OrderBook.BestBid?.Quantity);

            if (args.OrderBook.Bids != null)
            {
                foreach (OrderBookPrice bid in args.OrderBook.Bids.Take(5))
                {
                    _logger.LogInformation(
                        "ORDER BOOK BID {Symbol} BID Price:{Price} Quantity:{Quantity}",
                        args.OrderBook.Symbol,
                        bid.Price,
                        bid.Quantity);
                }
            }

            if (args.OrderBook.Asks != null)
            {
                foreach (OrderBookPrice ask in args.OrderBook.Asks.Take(5))
                {
                    _logger.LogInformation(
                        "ORDER BOOK ASK {Symbol} ASK Price:{Price} Quantity:{Quantity}",
                        args.OrderBook.Symbol,
                        ask.Price,
                        ask.Quantity);
                }
            }
        }

        private static void OnAccountCallback(
            AccountRealtimeState accountRealtimeState,
            AccountEventArgs args)
        {
            if (args.Account == null)
                return;

            accountRealtimeState.Update(args.Account);
        }

        private void OnKlineError(Runtime.Strategy strategy, Exception ex)
        {
            _logger.LogError(ex,
                "Kline subscription error for {Symbol} {Interval}",
                strategy.Symbol,
                strategy.KlineInterval);
        }

        private void OnTradeError(Runtime.Strategy strategy, Exception ex)
        {
            _logger.LogError(ex,
                "Trade subscription error for {Symbol}",
                strategy.Symbol);
        }

        private void OnOrderBookError(Runtime.Strategy strategy, Exception ex)
        {
            _logger.LogError(ex,
                "Order book subscription error for {Symbol}",
                strategy.Symbol);
        }

        private void OnAccountError(Credentials credentials, Exception ex)
        {
            _logger.LogError(ex,
                "Account subscription error for account {AccountName}",
                credentials.AccountName);
        }

        private static async Task WaitUntilCancelledCleanlyAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }
    }
}
