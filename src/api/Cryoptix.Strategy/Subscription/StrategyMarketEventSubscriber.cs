using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Subscription
{
    public sealed class StrategyMarketEventSubscriber(
        ILogger<StrategyMarketEventSubscriber> logger) : IStrategyMarketEventSubscriber
    {
        private readonly ILogger<StrategyMarketEventSubscriber> _logger = logger;

        public async Task<StrategyMarketEventSubscriptions> SubscribeAsync(
            Runtime.Strategy strategy,
            IExchangeSubscriptionApi subscriptionsApi,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentNullException.ThrowIfNull(subscriptionsApi);
            ArgumentNullException.ThrowIfNull(writer);

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            CancellationTokenSource sessionCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            IAsyncDisposable? klineSubscription = null;
            IAsyncDisposable? tradeSubscription = null;

            try
            {
                klineSubscription = await subscriptionsApi.SubscribeToKlineUpdatesAsync(
                    symbol: strategy.Symbol,
                    interval: strategy.KlineInterval,
                    onCallback: args => OnKlineCallback(strategy, writer, args),
                    onError: ex => OnKlineError(strategy, ex),
                    cancellationToken: sessionCancellationTokenSource.Token);

                tradeSubscription = await subscriptionsApi.SubscribeToTradesAsync(
                    symbol: strategy.Symbol,
                    onCallback: args => OnTradeCallback(strategy, writer, args),
                    onError: ex => OnTradeError(strategy, ex),
                    cancellationToken: sessionCancellationTokenSource.Token);

                CompositeAsyncDisposable compositeHandle = new(klineSubscription, tradeSubscription);
                Task completionTask = WaitUntilCancelledCleanlyAsync(sessionCancellationTokenSource.Token);

                _logger.LogInformation(
                    "Started subscriptions for {Symbol} {Interval}",
                    strategy.Symbol,
                    strategy.KlineInterval);

                return new StrategyMarketEventSubscriptions(
                    compositeHandle,
                    sessionCancellationTokenSource,
                    completionTask);
            }
            catch
            {
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
            ChannelWriter<MarketEvent> writer,
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
                        "Failed to enqueue live kline for {Symbol} {Interval}; channel closed.",
                        kline.Symbol,
                        kline.Interval);
                }
            }
        }

        private void OnTradeCallback(
            Runtime.Strategy strategy,
            ChannelWriter<MarketEvent> writer,
            TradeEventArgs args)
        {
            if (args.Trades == null || !args.Trades.Any())
            {
                _logger.LogWarning(
                    "Received trade update with no trades for {Symbol}",
                    strategy.Symbol);
                return;
            }

            foreach (Trade trade in args.Trades)
            {
                if (!writer.TryWrite(new TradeMarketEvent(trade)))
                {
                    _logger.LogWarning(
                        "Failed to enqueue live trade for {Symbol}; channel closed.",
                        trade.Symbol);
                }
            }
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
