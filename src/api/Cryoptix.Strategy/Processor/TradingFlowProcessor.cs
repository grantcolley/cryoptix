using Cryoptix.Market.Data;
using Cryoptix.Observer.Metrics;
using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Dispatcher;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Notification;
using Cryoptix.Strategy.Seeding;
using Cryoptix.Strategy.Snapshot;
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
        IStrategyEventChannelFactory strategyEventChannelFactory,
        ITradingFlowSessionAccessor tradingFlowSessionAccessor,
        IStrategyStatusNotifier strategyStatusNotifier,
        INotificationMetrics notificationMetrics,
        INotificationPump notificationPump) : IStrategyProcessor
    {
        public readonly StrategyProcessorType StrategyProcessorType = StrategyProcessorType.TradingFlow;

        private readonly ILogger<TradingFlowProcessor> _logger = logger;
        private readonly IStrategyMarketSeeder _strategyMarketSeeder = strategyMarketSeeder;
        private readonly IStrategyMarketEventSubscriber _strategyMarketEventSubscriber = strategyMarketEventSubscriber;
        private readonly IStrategyMarketEventDispatcher _strategyMarketEventDispatcher = strategyMarketEventDispatcher;
        private readonly IStrategyEventChannelFactory _strategyEventChannelFactory = strategyEventChannelFactory;
        private readonly ITradingFlowSessionAccessor _tradingFlowSessionAccessor = tradingFlowSessionAccessor;
        private readonly IStrategyStatusNotifier _strategyStatusNotifier = strategyStatusNotifier;
        private readonly INotificationMetrics _notificationMetrics = notificationMetrics;
        private readonly INotificationPump _notificationPump = notificationPump;

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

            Strategies.Strategy? initialStrategy = strategyAgentSession.GetStrategy();
            ValidateInitialStrategy(initialStrategy);

            StrategyProcessorSession strategyProcessorSession = new()
            {
                ExchangeApi = strategyAgentSession.ExchangeApi,
                Strategy = initialStrategy!,
                Credentials = strategyAgentSession.Credentials,
                OrderBookRealtimeState = new OrderBookRealtimeState(),
                AccountRealtimeState = new AccountRealtimeState(),
                Cache = new MarketDataCache(
                    maxTradesPerSymbol: initialStrategy.CacheMaxTradesPerSymbol,
                    maxKlinesPerSeries: initialStrategy.CacheMaxKlinesPerSeries,
                    maxIndicatorsPerSeries: initialStrategy.CacheMaxIndicatorsPerSeries,
                    maxSignalsPerSeries: initialStrategy.CacheMaxSignalsPerSeries)
            };

            _tradingFlowSessionAccessor.SetCurrent(strategyProcessorSession);

            StrategyEventChannels channels = _strategyEventChannelFactory.Create(initialStrategy);

            StrategyMarketEventSubscriptions? strategyMarketEventSubscriptions = null;
            Task processingTask = Task.CompletedTask;
            Task notificationTask = Task.CompletedTask;

            try
            {
                await _strategyStatusNotifier.NotifyStartedAsync(
                    strategyProcessorSession.Strategy,
                    cancellationToken);

                await _strategyMarketSeeder.SeedAsync(
                    strategy: strategyProcessorSession.Strategy,
                    restApi: strategyProcessorSession.ExchangeApi.RestApi!,
                    klineWriter: channels.Klines.Writer,
                    cache: strategyProcessorSession.Cache,
                    cancellationToken: cancellationToken);

                await _strategyStatusNotifier.NotifyMarketDataSnapshotAsync(cancellationToken);

                strategyMarketEventSubscriptions = await _strategyMarketEventSubscriber.SubscribeAsync(
                    strategy: strategyProcessorSession.Strategy,
                    credentials: strategyProcessorSession.Credentials,
                    subscriptionsApi: strategyProcessorSession.ExchangeApi.SubscriptionsApi!,
                    channels: channels,
                    orderBookRealtimeState: strategyProcessorSession.OrderBookRealtimeState,
                    accountRealtimeState: strategyProcessorSession.AccountRealtimeState,
                    cancellationToken: cancellationToken);

                processingTask = ProcessMarketEventsAsync(
                    strategyProcessorSession,
                    channels,
                    cancellationToken);

                notificationTask = _notificationPump.RunAsync(
                    strategy: strategyProcessorSession.Strategy,
                    channels,
                    cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyAgentSession.WaitForStrategyUpdateAsync(cancellationToken);
                    Task subscriptionCompletionTask = strategyMarketEventSubscriptions.Completion;

                    Task completed = await Task.WhenAny(
                        strategyUpdateTask,
                        subscriptionCompletionTask,
                        processingTask,
                        notificationTask);

                    if (completed == processingTask)
                    {
                        await processingTask;
                        break;
                    }

                    if (completed == notificationTask)
                    {
                        await notificationTask;
                        break;
                    }

                    if (completed == subscriptionCompletionTask)
                    {
                        await subscriptionCompletionTask;
                        break;
                    }

                    Strategies.Strategy? updatedStrategy = strategyAgentSession.GetStrategy();
                    if (updatedStrategy == null)
                    {
                        _logger.LogWarning("Received null strategy update; ignoring. (ignored)");
                        continue;
                    }

                    ValidateCompatibleStrategyUpdate(strategyProcessorSession.Strategy, updatedStrategy);

                    strategyProcessorSession.Strategy = updatedStrategy;

                    _logger.LogInformation(
                        "Applied strategy update for {Symbol}. Subscriptions unchanged.",
                        strategyProcessorSession.Strategy.Symbol);

                    await _strategyStatusNotifier.NotifyUpdatedAsync(
                        strategyProcessorSession.Strategy,
                        cancellationToken);
                }
            }
            finally
            {
                _tradingFlowSessionAccessor.ClearCurrent();

                if (strategyMarketEventSubscriptions != null)
                {
                    await strategyMarketEventSubscriptions.DisposeAsync();
                }

                channels.CompleteWriters();

                try
                {
                    await Task.WhenAll(processingTask, notificationTask);
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

            ChannelWriter<Kline> klineBroadcastWriter = channels.KlineBroadcasts.Writer;
            ChannelWriter<Trade> tradeBroadcastWriter = channels.TradeBroadcasts.Writer;

            int maxTradesPerPass = session.Strategy.StrategyProcessorMaxTradesPerPass;

            while (!cancellationToken.IsCancellationRequested)
            {
                bool processedAny = false;

                while (klineReader.TryRead(out KlineMarketEvent? klineEvent))
                {
                    processedAny = true;

                    if (!klineBroadcastWriter.TryWrite(klineEvent.Kline))
                    {
                        _logger.LogDebug(
                            "Dropped kline broadcast event for {Symbol} {Interval} due to broadcast channel pressure.",
                            klineEvent.Kline.Symbol,
                            klineEvent.Kline.Interval);

                        _notificationMetrics.RecordBroadcastDropKline(
                            klineEvent.Kline.Symbol,
                            klineEvent.Kline.Interval);
                    }

                    await _strategyMarketEventDispatcher.DispatchAsync(
                        session,
                        klineEvent,
                        channels,
                        cancellationToken);
                }

                int tradeBatchCount = 0;

                while (tradeBatchCount < maxTradesPerPass && tradeReader.TryRead(out TradeMarketEvent? tradeEvent))
                {
                    processedAny = true;
                    tradeBatchCount++;

                    if (!tradeBroadcastWriter.TryWrite(tradeEvent.Trade))
                    {
                        _logger.LogDebug(
                            "Dropped trade broadcast event for {Symbol} TradeId:{TradeId} due to broadcast channel pressure.",
                            tradeEvent.Trade.Symbol,
                            tradeEvent.Trade.Id);

                        _notificationMetrics.RecordBroadcastDropTrade(tradeEvent.Trade.Symbol);
                    }

                    await _strategyMarketEventDispatcher.DispatchAsync(
                        session,
                        tradeEvent,
                        channels,
                        cancellationToken);
                }

                if (klineReader.Completion.IsCompleted && tradeReader.Completion.IsCompleted)
                {
                    bool hasRemainingKlines = klineReader.TryPeek(out _);
                    bool hasRemainingTrades = tradeReader.TryPeek(out _);

                    if (!hasRemainingKlines && !hasRemainingTrades)
                        break;
                }

                if (processedAny)
                    continue;

                Task<bool> waitForKline = klineReader.WaitToReadAsync(cancellationToken).AsTask();
                Task<bool> waitForTrade = tradeReader.WaitToReadAsync(cancellationToken).AsTask();

                Task completed = await Task.WhenAny(waitForKline, waitForTrade);

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

        private static void ValidateInitialStrategy(Strategies.Strategy? strategy)
        {
            if (strategy == null)
                throw new InvalidOperationException("Strategy is required.");

            if (string.IsNullOrWhiteSpace(strategy.Symbol))
                throw new InvalidOperationException("Strategy symbol is required.");

            if (strategy.KlineInterval == default)
                throw new InvalidOperationException("Strategy kline interval is required.");
        }

        private static void ValidateCompatibleStrategyUpdate(Strategies.Strategy current, Strategies.Strategy updated)
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
