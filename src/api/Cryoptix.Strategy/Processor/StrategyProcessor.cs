using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Subscription;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Processor
{
    public class StrategyProcessor(
        ILogger<StrategyProcessor> logger,
        IStrategyEnginePairFactory strategyEnginePairFactory,
        IStrategyAnalysisContextFactory strategyAnalysisContextFactory,
        IStrategyMarketEventSubscriber strategyMarketEventSubscriber) : IStrategyProcessor
    {
        public readonly StrategyProcessorType StrategyProcessorType = StrategyProcessorType.TradingFlow;

        private readonly ILogger<StrategyProcessor> _logger = logger;
        private readonly IStrategyEnginePairFactory _strategyEnginePairFactory = strategyEnginePairFactory;
        private readonly IStrategyAnalysisContextFactory _strategyAnalysisContextFactory = strategyAnalysisContextFactory;
        private readonly IStrategyMarketEventSubscriber _strategyMarketEventSubscriber = strategyMarketEventSubscriber;

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
                    maxTradesPerSymbol: 10_000,
                    maxKlinesPerSeries: 5_000)
            };

            Channel<MarketEvent> marketEventChannel = Channel.CreateUnbounded<MarketEvent>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });

            StrategyMarketEventSubscriptions? strategyMarketEventSubscriptions = null;
            Task processingTask = Task.CompletedTask;

            try
            {
                await SeedStrategyAsync(
                    strategy: strategyProcessorSession.Strategy,
                    restApi: strategyProcessorSession.ExchangeApi.RestApi!,
                    writer: marketEventChannel.Writer,
                    cancellationToken: cancellationToken);

                strategyMarketEventSubscriptions = await _strategyMarketEventSubscriber.SubscribeAsync(
                    strategy: strategyProcessorSession.Strategy,
                    subscriptionsApi: strategyProcessorSession.ExchangeApi.SubscriptionsApi!,
                    writer: marketEventChannel.Writer,
                    cancellationToken: cancellationToken);

                processingTask = ProcessMarketEventsAsync(
                    strategyProcessorSession: strategyProcessorSession,
                    reader: marketEventChannel.Reader,
                    cancellationToken: cancellationToken);

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

                marketEventChannel.Writer.TryComplete();

                try
                {
                    await processingTask;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
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

        private async Task SeedStrategyAsync(
            Runtime.Strategy strategy,
            IExchangeRestApi restApi,
            ChannelWriter<MarketEvent> writer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DateTime endTime = DateTime.UtcNow;
            DateTime startTime = GetSeedStartTime(strategy, endTime);

            _logger.LogInformation(
                "Fetching historical klines for {Symbol} {Interval} from {Start:u} to {End:u}",
                strategy.Symbol,
                strategy.KlineInterval,
                startTime,
                endTime);

            List<Kline> historicalKlines = await restApi.GetKlinesAsync(
                symbol: strategy.Symbol!,
                interval: strategy.KlineInterval,
                startTime: startTime,
                endTime: endTime,
                limit: null,
                cancellationToken: cancellationToken);

            foreach (Kline kline in historicalKlines.OrderBy(k => k.OpenTime))
            {
                await writer.WriteAsync(new KlineMarketEvent(kline, MarketEventSource.Seed), cancellationToken);
            }

            _logger.LogInformation(
                "Seeded {Count} klines for {Symbol} {Interval}",
                historicalKlines.Count,
                strategy.Symbol,
                strategy.KlineInterval);
        }

        private async Task ProcessMarketEventsAsync(
            StrategyProcessorSession strategyProcessorSession,
            ChannelReader<MarketEvent> reader,
            CancellationToken cancellationToken)
        {
            await foreach (MarketEvent marketEvent in reader.ReadAllAsync(cancellationToken))
            {
                switch (marketEvent)
                {
                    case KlineMarketEvent klineEvent:
                        await ProcessKlineEventAsync(strategyProcessorSession, klineEvent, cancellationToken);
                        break;

                    case TradeMarketEvent tradeEvent:
                        await ProcessTradeEventAsync(strategyProcessorSession, tradeEvent, cancellationToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown market event type {EventType}", marketEvent.GetType().Name);
                        break;
                }
            }
        }

        private async Task ProcessKlineEventAsync(
            StrategyProcessorSession session,
            KlineMarketEvent marketEvent,
            CancellationToken cancellationToken)
        {
            KlineUpsertResult upsertResult = session.Cache.UpsertKline(marketEvent.Kline);

            StrategyAnalysisContext context = _strategyAnalysisContextFactory.CreateForKline(session, marketEvent);

            IStrategyEnginePair enginePair =
                _strategyEnginePairFactory.Get(context.Strategy.StrategyEngineType);

            IndicatorComputationResult indicators =
                await enginePair.IndicatorEngine.ComputeAsync(context, cancellationToken);

            SignalEvaluationResult signal =
                await enginePair.SignalEngine.EvaluateAsync(context, indicators, cancellationToken);

            _logger.LogInformation(
                "KLINE {Source} {Symbol} {Interval} OpenTime:{OpenTime:u} Inserted:{Inserted} Updated:{Updated} Signal:{Signal} Reason:{Reason}",
                marketEvent.Source,
                marketEvent.Kline.Symbol,
                marketEvent.Kline.Interval,
                marketEvent.Kline.OpenTime,
                upsertResult.Inserted,
                upsertResult.Updated,
                signal.Signal,
                signal.Reason);

            await HandleSignalAsync(context, signal, cancellationToken);
        }

        private async Task ProcessTradeEventAsync(
            StrategyProcessorSession session,
            TradeMarketEvent marketEvent,
            CancellationToken cancellationToken)
        {
            bool added = session.Cache.AddTrade(marketEvent.Trade);
            if (!added)
            {
                _logger.LogDebug(
                    "Ignored duplicate trade {TradeId} for {Symbol}",
                    marketEvent.Trade.Id,
                    marketEvent.Trade.Symbol);

                return;
            }

            StrategyAnalysisContext context = _strategyAnalysisContextFactory.CreateForTrade(session, marketEvent);

            IStrategyEnginePair enginePair =
                _strategyEnginePairFactory.Get(context.Strategy.StrategyEngineType);

            IndicatorComputationResult indicators =
                await enginePair.IndicatorEngine.ComputeAsync(context, cancellationToken);

            SignalEvaluationResult signal =
                await enginePair.SignalEngine.EvaluateAsync(context, indicators, cancellationToken);

            _logger.LogInformation(
                "TRADE {Symbol} TradeId:{TradeId} Signal:{Signal} Reason:{Reason}",
                marketEvent.Trade.Symbol,
                marketEvent.Trade.Id,
                signal.Signal,
                signal.Reason);

            await HandleSignalAsync(context, signal, cancellationToken);
        }

        private Task HandleSignalAsync(
            StrategyAnalysisContext context,
            SignalEvaluationResult signal,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (signal.Signal == StrategySignal.None)
                return Task.CompletedTask;

            _logger.LogInformation(
                "Signal generated for {Symbol} [{StrategyType}]: {Signal}. Reason: {Reason}",
                context.Strategy.Symbol,
                context.Strategy.StrategyProcessorType,
                signal.Signal,
                signal.Reason);

            return Task.CompletedTask;
        }

        private static DateTime GetSeedStartTime(Runtime.Strategy strategy, DateTime endTimeUtc)
        {
            return endTimeUtc.AddDays(-2);
        }
    }
}
