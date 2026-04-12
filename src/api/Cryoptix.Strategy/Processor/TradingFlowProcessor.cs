using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Dispatcher;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Seeding;
using Cryoptix.Strategy.Subscription;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Processor
{
    public class TradingFlowProcessor(
        ILogger<TradingFlowProcessor> logger,
        IStrategyMarketSeeder strategyMarketSeeder,
        IStrategyMarketEventSubscriber strategyMarketEventSubscriber,
        IStrategyMarketEventDispatcher strategyMarketEventDispatcher,
        IStrategyEventChannelFactory strategyEventChannelFactory) : IStrategyProcessor
    {
        public readonly StrategyProcessorType StrategyProcessorType = StrategyProcessorType.TradingFlow;

        private readonly ILogger<TradingFlowProcessor> _logger = logger;
        private readonly IStrategyMarketSeeder _strategyMarketSeeder = strategyMarketSeeder;
        private readonly IStrategyMarketEventSubscriber _strategyMarketEventSubscriber = strategyMarketEventSubscriber;
        private readonly IStrategyMarketEventDispatcher _strategyMarketEventDispatcher = strategyMarketEventDispatcher;
        private readonly IStrategyEventChannelFactory _strategyEventChannelFactory = strategyEventChannelFactory;

        public async Task ExecuteAsync(StrategyAgentSession strategyAgentSession, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategyAgentSession);
            ArgumentNullException.ThrowIfNull(strategyAgentSession.ExchangeApi);
            ArgumentNullException.ThrowIfNull(strategyAgentSession.ExchangeApi.RestApi);
            ArgumentNullException.ThrowIfNull(strategyAgentSession.ExchangeApi.SubscriptionsApi);

            if (strategyAgentSession.GetStrategy is null)
                throw new ArgumentNullException($"{nameof(strategyAgentSession)}.GetStrategy");
            if (strategyAgentSession.WaitForStrategyUpdateAsync is null)
                throw new ArgumentNullException($"{nameof(strategyAgentSession)}.WaitForStrategyUpdateAsync");

            Runtime.Strategy? initialStrategy = strategyAgentSession.GetStrategy();
            ValidateInitialStrategy(initialStrategy);

            StrategyProcessorSession strategyProcessorSession = new()
            {
                ExchangeApi = strategyAgentSession.ExchangeApi,
                Strategy = initialStrategy!,
                Cache = new MarketDataCache(
                    maxTradesPerSymbol: initialStrategy.CacheMaxTradesPerSymbol,
                    maxKlinesPerSeries: initialStrategy.CacheMaxKlinesPerSeries)
            };

            StrategyEventChannels channels = _strategyEventChannelFactory.Create();

            StrategyMarketEventSubscriptions? strategyMarketEventSubscriptions = null;
            Task processingTask = Task.CompletedTask;

            try
            {
                await _strategyMarketSeeder.SeedAsync(
                    strategy: strategyProcessorSession.Strategy,
                    restApi: strategyProcessorSession.ExchangeApi.RestApi!,
                    klineWriter: channels.Klines.Writer,
                    cancellationToken: cancellationToken);

                strategyMarketEventSubscriptions = await _strategyMarketEventSubscriber.SubscribeAsync(
                    strategy: strategyProcessorSession.Strategy,
                    subscriptionsApi: strategyProcessorSession.ExchangeApi.SubscriptionsApi!,
                    klineWriter: channels.Klines.Writer,
                    tradeWriter: channels.Trades.Writer,
                    cancellationToken: cancellationToken);

                processingTask = ProcessMarketEventsAsync(
                    strategyProcessorSession,
                    channels,
                    cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyAgentSession.WaitForStrategyUpdateAsync(cancellationToken);
                    Task subscriptionCompletionTask = strategyMarketEventSubscriptions.Completion;

                    Task completed = await Task.WhenAny(
                        strategyUpdateTask,
                        subscriptionCompletionTask,
                        processingTask);

                    if (completed == processingTask)
                    {
                        await processingTask;
                        break;
                    }

                    if (completed == subscriptionCompletionTask)
                    {
                        await subscriptionCompletionTask;
                        break;
                    }

                    Runtime.Strategy? updatedStrategy = strategyAgentSession.GetStrategy();
                    if (updatedStrategy == null)
                    {
                        _logger.LogWarning("Received null strategy update; ignoring.");
                        continue;
                    }

                    ValidateCompatibleStrategyUpdate(strategyProcessorSession.Strategy, updatedStrategy);

                    strategyProcessorSession.Strategy = updatedStrategy;

                    _logger.LogInformation(
                        "Applied strategy update for {Symbol}. Subscriptions unchanged.",
                        strategyProcessorSession.Strategy.Symbol);
                }
            }
            finally
            {
                if (strategyMarketEventSubscriptions != null)
                {
                    await strategyMarketEventSubscriptions.DisposeAsync();
                }

                channels.CompleteWriters();

                try
                {
                    await processingTask;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                }
            }
        }

        private async Task ProcessMarketEventsAsync(
            StrategyProcessorSession session,
            StrategyEventChannels channels,
            CancellationToken cancellationToken)
        {
            ChannelReader<KlineMarketEvent> klineReader = channels.Klines.Reader;
            ChannelReader<TradeMarketEvent> tradeReader = channels.Trades.Reader;

            int maxTradesPerPass = session.Strategy.StrategyProcessorMaxTradesPerPass;

            while (!cancellationToken.IsCancellationRequested)
            {
                bool processedAny = false;

                // Drain klines first: they are higher priority.
                while (klineReader.TryRead(out KlineMarketEvent? klineEvent))
                {
                    processedAny = true;
                    await _strategyMarketEventDispatcher.DispatchAsync(
                        session,
                        klineEvent,
                        cancellationToken);
                }

                // Then process a batch of trades.
                int tradeBatchCount = 0;

                while (tradeBatchCount < maxTradesPerPass && tradeReader.TryRead(out TradeMarketEvent? tradeEvent))
                {
                    processedAny = true;
                    tradeBatchCount++;

                    await _strategyMarketEventDispatcher.DispatchAsync(
                        session,
                        tradeEvent,
                        cancellationToken);
                }

                // Exit when both channels are done and empty.
                if (klineReader.Completion.IsCompleted && tradeReader.Completion.IsCompleted)
                {
                    bool hasRemainingKlines = klineReader.TryPeek(out _);
                    bool hasRemainingTrades = tradeReader.TryPeek(out _);

                    if (!hasRemainingKlines && !hasRemainingTrades)
                        break;
                }

                if (processedAny)
                    continue;

                // Wait for either channel to have data.
                Task<bool> waitForKline = klineReader.WaitToReadAsync(cancellationToken).AsTask();
                Task<bool> waitForTrade = tradeReader.WaitToReadAsync(cancellationToken).AsTask();

                Task completed = await Task.WhenAny(waitForKline, waitForTrade);

                // Propagate faults/cancellation.
                if (completed == waitForKline)
                {
                    await waitForKline;
                }
                else
                {
                    await waitForTrade;
                }
            }
        }

        private static void ValidateInitialStrategy(Runtime.Strategy? strategy)
        {
            if (strategy == null)
                throw new InvalidOperationException("Strategy is required.");

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            if (strategy.KlineInterval == default)
                throw new InvalidOperationException("Strategy kline interval is required.");
        }

        private static void ValidateCompatibleStrategyUpdate(Runtime.Strategy current, Runtime.Strategy updated)
        {
            if (!string.Equals(current.Symbol, updated.Symbol, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Strategy update cannot change Symbol while processor is running.");
            }

            if (current.KlineInterval != updated.KlineInterval)
            {
                throw new InvalidOperationException("Strategy update cannot change Interval while processor is running.");
            }
        }
    }
}
