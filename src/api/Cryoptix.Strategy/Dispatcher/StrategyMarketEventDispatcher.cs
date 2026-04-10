using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Dispatcher
{
    public sealed class StrategyMarketEventDispatcher(
        ILogger<StrategyMarketEventDispatcher> logger,
        IStrategyAnalysisContextFactory strategyAnalysisContextFactory,
        IStrategyEnginePairFactory strategyEnginePairFactory) : IStrategyMarketEventDispatcher
    {
        private readonly ILogger<StrategyMarketEventDispatcher> _logger = logger;
        private readonly IStrategyAnalysisContextFactory _strategyAnalysisContextFactory = strategyAnalysisContextFactory;
        private readonly IStrategyEnginePairFactory _strategyEnginePairFactory = strategyEnginePairFactory;

        public async Task DispatchAsync(
            StrategyProcessorSession session,
            MarketEvent marketEvent,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(marketEvent);

            cancellationToken.ThrowIfCancellationRequested();

            switch (marketEvent)
            {
                case KlineMarketEvent klineEvent:
                    await DispatchKlineAsync(session, klineEvent, cancellationToken);
                    break;

                case TradeMarketEvent tradeEvent:
                    await DispatchTradeAsync(session, tradeEvent, cancellationToken);
                    break;

                default:
                    _logger.LogWarning(
                        "Unknown market event type {EventType}",
                        marketEvent.GetType().Name);
                    break;
            }
        }

        private async Task DispatchKlineAsync(
            StrategyProcessorSession session,
            KlineMarketEvent marketEvent,
            CancellationToken cancellationToken)
        {
            KlineUpsertResult upsertResult = session.Cache.UpsertKline(marketEvent.Kline);

            StrategyAnalysisContext context =
                _strategyAnalysisContextFactory.CreateForKline(session, marketEvent);

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

        private async Task DispatchTradeAsync(
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

            StrategyAnalysisContext context =
                _strategyAnalysisContextFactory.CreateForTrade(session, marketEvent);

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

            // Future hook:
            // return _orderExecutor.ExecuteAsync(context, signal, cancellationToken);

            return Task.CompletedTask;
        }
    }
}
