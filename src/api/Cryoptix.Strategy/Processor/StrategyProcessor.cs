using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Runtime;
using Cryoptix.Strategy.Subscriptions;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Processor
{
    public class StrategyProcessor(ILogger<StrategyProcessor> logger) : IStrategyProcessor
    {
        public readonly StrategyProcessorType StrategyProcessorType = StrategyProcessorType.MovingAverage;

        private readonly ILogger<StrategyProcessor> _logger = logger;

        public async Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategyRuntime);
            ArgumentNullException.ThrowIfNull(strategyRuntime.ExchangeApi);
            ArgumentNullException.ThrowIfNull(strategyRuntime.ExchangeApi.RestApi);
            ArgumentNullException.ThrowIfNull(strategyRuntime.ExchangeApi.SubscriptionsApi);
            if (strategyRuntime.GetStrategy is null) throw new ArgumentNullException($"{nameof(strategyRuntime)}.GetStrategy()");
            if (strategyRuntime.WaitForStrategyUpdateAsync is null) throw new ArgumentNullException($"{nameof(strategyRuntime)}.WaitForStrategyUpdateAsync");

            Runtime.Strategy? initialStrategy = strategyRuntime.GetStrategy();

            ValidateInitialStrategy(initialStrategy);

            ExchangeApi exchangeApi = strategyRuntime.ExchangeApi;

            // Mutable run-local strategy reference. Updates only change indicator params.
            Runtime.Strategy currentStrategy = initialStrategy!;

            // Single-reader processing queue; websocket callbacks only enqueue.
            Channel<MarketEvent> marketDataChannel = Channel.CreateUnbounded<MarketEvent>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken runToken = linkedCts.Token;

            SubscriptionSession? subscriptionSession = null;
            Task processingTask = Task.CompletedTask;

            try
            {
                // 1. Seed from REST before starting live subscriptions.
                await SeedStrategyAsync(
                    exchangeApi.RestApi!,
                    currentStrategy,
                    marketDataChannel.Writer,
                    runToken);

                // 2. Start live subscriptions after seeding.
                subscriptionSession = await StartStrategySubscriptionsAsync(
                    exchangeApi.SubscriptionsApi!,
                    currentStrategy,
                    marketDataChannel.Writer,
                    runToken);

                // 3. Start processing loop outside websocket callbacks.
                processingTask = ProcessMarketDataLoopAsync(
                    marketDataChannel.Reader,
                    () => currentStrategy,
                    runToken);

                while (!runToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyRuntime.WaitForStrategyUpdateAsync(runToken);
                    Task sessionCompletionTask = subscriptionSession.Completion;

                    Task completed = await Task.WhenAny(strategyUpdateTask, sessionCompletionTask, processingTask);

                    if (completed == processingTask)
                    {
                        // Propagate processing failures/cancellation
                        await processingTask;
                        break;
                    }

                    if (completed == sessionCompletionTask)
                    {
                        // Propagate subscription failures/cancellation
                        await sessionCompletionTask;
                        break;
                    }

                    // Strategy update occurred. Only indicator params are allowed to change.
                    Runtime.Strategy? updatedStrategy = strategyRuntime.GetStrategy();
                    if (updatedStrategy is null)
                    {
                        _logger.LogWarning("Received null strategy update; ignoring.");
                        continue;
                    }

                    ValidateCompatibleStrategyUpdate(currentStrategy, updatedStrategy);

                    currentStrategy = updatedStrategy;
                    _logger.LogInformation(
                        "Applied strategy update for {Symbol}. Subscriptions unchanged.",
                        currentStrategy.Symbol);
                }
            }
            finally
            {
                if (subscriptionSession is not null)
                {
                    await subscriptionSession.DisposeAsync();
                }

                marketDataChannel.Writer.TryComplete();

                try
                {
                    await processingTask;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Expected on shutdown.
                }
            }
        }

        private static void ValidateInitialStrategy(Runtime.Strategy? strategy)
        {
            if (strategy is null) throw new InvalidOperationException("Strategy is required.");
            if (string.IsNullOrWhiteSpace(strategy.Symbol)) throw new InvalidOperationException("Strategy symbol is required.");

            // Add any other required checks, for example interval:
            // if (strategy.Interval == default) throw new InvalidOperationException("Strategy interval is required.");
        }

        private static void ValidateCompatibleStrategyUpdate(Runtime.Strategy current, Runtime.Strategy updated)
        {
            if (!string.Equals(current.Symbol, updated.Symbol, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Strategy update cannot change Symbol while processor is running.");

            // If interval/subscriptions must remain unchanged, validate that too:
            // if (current.Interval != updated.Interval)
            // {
            //     throw new InvalidOperationException("Strategy update cannot change Interval while processor is running.");
            // }
        }

        private async Task SeedStrategyAsync(
            IExchangeRestApi restApi,
            Runtime.Strategy strategy,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Replace with your actual interval / lookback rules.
            KlineInterval interval = strategy.KlineInterval;
            DateTime endTime = DateTime.UtcNow;
            DateTime startTime = endTime.AddDays(-2);

            _logger.LogInformation(
                "Fetching historical klines for {Symbol} {klineIinterval} from {StartTime:u} to {EndTime:u}",
                strategy.Symbol,
                interval,
                startTime,
                endTime);

            List<Kline> klines = await restApi.GetKlinesAsync(
                symbol: strategy.Symbol,
                interval: interval,
                startTime: startTime,
                endTime: endTime,
                limit: null,
                cancellationToken: cancellationToken);

            foreach (Kline kline in klines.OrderBy(k => k.OpenTime))
            {
                await writer.WriteAsync(new KlineMarketEvent(kline), cancellationToken);
            }

            _logger.LogInformation(
                "Seeded {Count} historical klines for {Symbol} {Interval}",
                klines.Count,
                strategy.Symbol,
                interval);
        }

        private async Task<SubscriptionSession> StartStrategySubscriptionsAsync(
            IExchangeSubscriptionApi subscriptionsApi,
            Runtime.Strategy strategy,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(strategy.Symbol)) throw new InvalidOperationException("Strategy symbol is required.");

            CancellationTokenSource sessionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                IAsyncDisposable klineSubscription = await subscriptionsApi.SubscribeToKlineUpdatesAsync(
                    symbol: strategy.Symbol,
                    interval: strategy.KlineInterval,
                    onCallback: args =>
                    {
                        foreach (Kline kline in args.Klines)
                        {
                            if (!writer.TryWrite(new KlineMarketEvent(kline)))
                            {
                                _logger.LogWarning("Dropped kline event because the channel is closed.");
                            }
                        }
                    },
                    onError: ex => _logger.LogError(ex, "Kline subscription error for {Symbol}", strategy.Symbol),
                    cancellationToken: sessionCts.Token);

                IAsyncDisposable tradeSubscription = await subscriptionsApi.SubscribeToTradesAsync(
                    symbol: strategy.Symbol,
                    onCallback: args =>
                    {
                        foreach (Trade trade in args.Trades)
                        {
                            if (!writer.TryWrite(new TradeMarketEvent(trade)))
                            {
                                _logger.LogWarning("Dropped trade event because the channel is closed.");
                            }
                        }
                    },
                    onError: ex => _logger.LogError(ex, "Trade subscription error for {Symbol}", strategy.Symbol),
                    cancellationToken: sessionCts.Token);

                IAsyncDisposable compositeHandle = new CompositeAsyncDisposable(klineSubscription, tradeSubscription);

                Task completionTask = WaitUntilCancelledCleanlyAsync(sessionCts.Token);

                return new SubscriptionSession(compositeHandle, sessionCts, completionTask);
            }
            catch
            {
                sessionCts.Dispose();
                throw;
            }
        }

        private async Task ProcessMarketDataLoopAsync(
            ChannelReader<MarketEvent> reader,
            Func<Runtime.Strategy> getStrategy,
            CancellationToken cancellationToken)
        {
            await foreach (MarketEvent marketEvent in reader.ReadAllAsync(cancellationToken))
            {
                Runtime.Strategy strategy = getStrategy();

                switch (marketEvent)
                {
                    case KlineMarketEvent km:
                        await ProcessKlineAsync(strategy, km.Kline, cancellationToken);
                        break;

                    case TradeMarketEvent tm:
                        await ProcessTradeAsync(strategy, tm.Trade, cancellationToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown market event type: {EventType}", marketEvent.GetType().Name);
                        break;
                }
            }
        }

        private Task ProcessKlineAsync(Runtime.Strategy strategy, Kline kline, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Do indicator calculations here, outside websocket callback.
            _logger.LogInformation(
                "KLINE {Symbol} {Interval} O:{Open} H:{High} L:{Low} C:{Close} Final:{Final}",
                kline.Symbol,
                kline.Interval,
                kline.Open,
                kline.High,
                kline.Low,
                kline.Close,
                kline.Final);

            // Example:
            // _indicatorEngine.UpdateFromKline(strategy, kline);
            // var signal = _strategyEngine.Evaluate(strategy);
            // ...
            return Task.CompletedTask;
        }

        private Task ProcessTradeAsync(Runtime.Strategy strategy, Trade trade, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "TRADE {Time:u} {Symbol} {Price} {QuoteQuantity}",
                trade.Time,
                trade.Symbol,
                trade.Price,
                trade.QuoteQuantity);

            // Example:
            // _indicatorEngine.UpdateFromTrade(strategy, trade);
            // var signal = _strategyEngine.Evaluate(strategy);
            // ...
            return Task.CompletedTask;
        }

        private static async Task WaitUntilCancelledCleanlyAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Expected; treat cancellation as normal completion.
            }
        }

        private abstract record MarketEvent;
        private sealed record KlineMarketEvent(Kline Kline) : MarketEvent;
        private sealed record TradeMarketEvent(Trade Trade) : MarketEvent;

        private sealed class CompositeAsyncDisposable(params IAsyncDisposable[] inner) : IAsyncDisposable
        {
            private readonly IAsyncDisposable[] _inner = inner;
            private int _disposed;

            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;

                List<Exception>? exceptions = null;

                foreach (IAsyncDisposable disposable in _inner.Reverse())
                {
                    try
                    {
                        await disposable.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                    }
                }

                if (exceptions is { Count: > 0 })
                    throw new AggregateException(exceptions);
            }
        }
    }
}
