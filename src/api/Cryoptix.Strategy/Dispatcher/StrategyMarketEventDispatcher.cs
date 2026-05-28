using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.Signal;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Dispatcher
{
    public sealed class StrategyMarketEventDispatcher(
        ILogger<StrategyMarketEventDispatcher> logger,
        IStrategyAnalysisContextFactory strategyAnalysisContextFactory,
        IStrategyEnginePairFactory strategyEnginePairFactory,
        IStrategySignalHandler strategySignalHandler) : IStrategyMarketEventDispatcher
    {
        private readonly ILogger<StrategyMarketEventDispatcher> _logger = logger;
        private readonly IStrategyAnalysisContextFactory _strategyAnalysisContextFactory = strategyAnalysisContextFactory;
        private readonly IStrategyEnginePairFactory _strategyEnginePairFactory = strategyEnginePairFactory;
        private readonly IStrategySignalHandler _strategySignalHandler = strategySignalHandler;

        public async Task DispatchAsync(
            StrategyProcessorSession session,
            MarketEvent marketEvent,
            Channel.StrategyEventChannels channels,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(marketEvent);

            cancellationToken.ThrowIfCancellationRequested();

            switch (marketEvent)
            {
                case KlineMarketEvent klineEvent:
                    await DispatchKlineAsync(session, klineEvent, channels, cancellationToken);
                    break;

                case TradeMarketEvent tradeEvent:
                    await DispatchTradeAsync(session, tradeEvent, channels, cancellationToken);
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
            Channel.StrategyEventChannels channels,
            CancellationToken cancellationToken)
        {
            KlineUpsertResult upsertKlineResult = session.Cache.UpsertKline(marketEvent.Kline);

            StrategyAnalysisContext context =
                _strategyAnalysisContextFactory.CreateForKline(session, marketEvent);

            IStrategyEnginePair enginePair =
                _strategyEnginePairFactory.Get(context.Strategy.StrategyEngineType);

            IndicatorComputationResult indicatorsResult =
                await enginePair.IndicatorEngine.ComputeAsync(context, cancellationToken);

            // Broadcast computed indicators for klines
            if (indicatorsResult.Indicators.TimestampUtc != DateTime.MinValue)
            {
                session.Cache.UpsertIndicators(context.Strategy.Symbol!, indicatorsResult.Indicators);

                if (!channels.IndicatorsBroadcasts.Writer.TryWrite(indicatorsResult.Indicators))
                {
                    _logger.LogDebug(
                        "Dropped indicators broadcast for {Symbol} due to channel pressure.",
                        context.Strategy.Symbol);
                }
            }

            SignalEvaluationResult signal =
                await enginePair.SignalEngine.EvaluateAsync(context, indicatorsResult, cancellationToken);

            if (signal.Signal.SignalType != SignalType.None)
            {
                session.Cache.UpsertSignal(context.Strategy.Symbol!, signal.Signal);

                // Broadcast signal
                if (!channels.SignalBroadcasts.Writer.TryWrite(signal.Signal))
                {
                    _logger.LogDebug(
                        "Dropped signal broadcast for {Symbol} due to channel pressure.",
                        context.Strategy.Symbol);
                }
            }

            _logger.LogInformation(
                "KLINE {Source} {Symbol} {Interval} OpenTime:{OpenTime:u} CloseTime:{CloseTime:u} Open:{Open} Close:{Close}",
                marketEvent.Source,
                marketEvent.Kline.Symbol,
                marketEvent.Kline.Interval,
                marketEvent.Kline.OpenTime,
                marketEvent.Kline.CloseTime,
                marketEvent.Kline.Open,
                marketEvent.Kline.Close);

            await _strategySignalHandler.HandleAsync(context, signal, cancellationToken);
        }

        private async Task DispatchTradeAsync(
            StrategyProcessorSession session,
            TradeMarketEvent marketEvent,
            Channel.StrategyEventChannels channels,
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

            IndicatorComputationResult indicatorsResult =
                await enginePair.IndicatorEngine.ComputeAsync(context, cancellationToken);

            if (indicatorsResult.Indicators.TimestampUtc != DateTime.MinValue)
            {
                // Upsert and broadcast indicators for trades as well
                session.Cache.UpsertIndicators(context.Strategy.Symbol!, indicatorsResult.Indicators);

                if (!channels.IndicatorsBroadcasts.Writer.TryWrite(indicatorsResult.Indicators))
                {
                    _logger.LogDebug(
                        "Dropped indicators broadcast for {Symbol} due to channel pressure.",
                        context.Strategy.Symbol);
                }
            }

            SignalEvaluationResult signal =
                await enginePair.SignalEngine.EvaluateAsync(context, indicatorsResult, cancellationToken);

            if (signal.Signal.SignalType != SignalType.None)
            {
                session.Cache.UpsertSignal(context.Strategy.Symbol!, signal.Signal);

                if (!channels.SignalBroadcasts.Writer.TryWrite(signal.Signal))
                {
                    _logger.LogDebug(
                        "Dropped signal broadcast for {Symbol} due to channel pressure.",
                        context.Strategy.Symbol);
                }
            }

            _logger.LogInformation(
                "TRADE {Symbol} TradeId:{TradeId} Time:{Time:u} Price:{Price} BaseQuantity:{BaseQuantity} QuoteQuantity:{QuoteQuantity}",
                marketEvent.Trade.Symbol,
                marketEvent.Trade.Id,
                marketEvent.Trade.Time,
                marketEvent.Trade.Price,
                marketEvent.Trade.BaseQuantity,
                marketEvent.Trade.QuoteQuantity);

            await _strategySignalHandler.HandleAsync(context, signal, cancellationToken);
        }
    }
}
