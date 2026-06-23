using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Event;

namespace Cryoptix.Strategy.Channel
{
    /// <summary>
    /// Represents the strategy event channel factory.
    /// </summary>
    public sealed class StrategyEventChannelFactory(
        StrategyChannelOptions options) : IStrategyEventChannelFactory
    {
        private readonly StrategyChannelOptions _options = options;

        /// <summary>
        /// Executes the create operation.
        /// </summary>
        /// <returns>The create result.</returns>
        public StrategyEventChannels Create()
        {
            return Create(_options.KlineCapacity, _options.KlineFullMode, _options.TradeCapacity, _options.TradeFullMode, _options.KlineBroadcastCapacity, _options.KlineBroadcastFullMode, _options.TradeBroadcastCapacity, _options.TradeBroadcastFullMode, _options.IndicatorsBroadcastCapacity, _options.IndicatorsBroadcastFullMode, _options.SignalBroadcastCapacity, _options.SignalBroadcastFullMode);
        }

        /// <summary>
        /// Executes the create operation.
        /// </summary>
        /// <param name="strategy">The strategy value.</param>
        /// <returns>The create result.</returns>
        public StrategyEventChannels Create(Strategies.Strategy strategy)
        {
            int klineCapacity = strategy.SubscriptionChannelKlineCapacity > 0 ? strategy.SubscriptionChannelKlineCapacity : _options.KlineCapacity;
            System.Threading.Channels.BoundedChannelFullMode klineFullMode = strategy.SubscriptionChannelKlineFullMode;
            int tradeCapacity = strategy.SubscriptionChannelTradeCapacity > 0 ? strategy.SubscriptionChannelTradeCapacity : _options.TradeCapacity;
            System.Threading.Channels.BoundedChannelFullMode tradeFullMode = strategy.SubscriptionChannelTradeFullMode;
            int klineBroadcastCapacity = strategy.KlineBroadcastCapacity > 0 ? strategy.KlineBroadcastCapacity : _options.KlineBroadcastCapacity;
            System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode = strategy.KlineBroadcastFullMode;
            int tradeBroadcastCapacity = strategy.TradeBroadcastCapacity > 0 ? strategy.TradeBroadcastCapacity : _options.TradeBroadcastCapacity;
            System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode = strategy.TradeBroadcastFullMode;

            int indicatorsBroadcastCapacity = strategy.IndicatorsBroadcastCapacity > 0 ? strategy.IndicatorsBroadcastCapacity : _options.IndicatorsBroadcastCapacity;
            System.Threading.Channels.BoundedChannelFullMode indicatorsBroadcastFullMode = strategy.IndicatorsBroadcastFullMode;

            int signalBroadcastCapacity = strategy.SignalBroadcastCapacity > 0 ? strategy.SignalBroadcastCapacity : _options.SignalBroadcastCapacity;
            System.Threading.Channels.BoundedChannelFullMode signalBroadcastFullMode = strategy.SignalBroadcastFullMode;

            return Create(klineCapacity, klineFullMode, tradeCapacity, tradeFullMode, klineBroadcastCapacity, klineBroadcastFullMode, tradeBroadcastCapacity, tradeBroadcastFullMode, indicatorsBroadcastCapacity, indicatorsBroadcastFullMode, signalBroadcastCapacity, signalBroadcastFullMode);
        }

        /// <summary>
        /// Executes the create operation.
        /// </summary>
        /// <param name="klineCapacity">The kline capacity value.</param>
        /// <param name="klineFullMode">The kline full mode value.</param>
        /// <param name="tradeCapacity">The trade capacity value.</param>
        /// <param name="tradeFullMode">The trade full mode value.</param>
        /// <param name="klineBroadcastCapacity">The kline broadcast capacity value.</param>
        /// <param name="klineBroadcastFullMode">The kline broadcast full mode value.</param>
        /// <param name="tradeBroadcastCapacity">The trade broadcast capacity value.</param>
        /// <param name="tradeBroadcastFullMode">The trade broadcast full mode value.</param>
        /// <param name="indicatorsBroadcastCapacity">The indicators broadcast capacity value.</param>
        /// <param name="indicatorsBroadcastFullMode">The indicators broadcast full mode value.</param>
        /// <param name="signalBroadcastCapacity">The signal broadcast capacity value.</param>
        /// <param name="signalBroadcastFullMode">The signal broadcast full mode value.</param>
        /// <returns>The create result.</returns>
        public StrategyEventChannels Create(int klineCapacity, System.Threading.Channels.BoundedChannelFullMode klineFullMode, int tradeCapacity, System.Threading.Channels.BoundedChannelFullMode tradeFullMode, int klineBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode, int tradeBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode, int indicatorsBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode indicatorsBroadcastFullMode, int signalBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode signalBroadcastFullMode)
        {
            System.Threading.Channels.Channel<KlineMarketEvent> klineChannel = System.Threading.Channels.Channel.CreateBounded<KlineMarketEvent>(
                new System.Threading.Channels.BoundedChannelOptions(klineCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                    FullMode = klineFullMode
                });

            System.Threading.Channels.Channel<TradeMarketEvent> tradeChannel = System.Threading.Channels.Channel.CreateBounded<TradeMarketEvent>(
                new System.Threading.Channels.BoundedChannelOptions(tradeCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                    FullMode = tradeFullMode
                });

            System.Threading.Channels.Channel<Kline> klineBroadcastChannel = System.Threading.Channels.Channel.CreateBounded<Kline>(
                new System.Threading.Channels.BoundedChannelOptions(klineBroadcastCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    AllowSynchronousContinuations = false,
                    FullMode = klineBroadcastFullMode
                });

            System.Threading.Channels.Channel<Trade> tradeBroadcastChannel = System.Threading.Channels.Channel.CreateBounded<Trade>(
                new System.Threading.Channels.BoundedChannelOptions(tradeBroadcastCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    AllowSynchronousContinuations = false,
                    FullMode = tradeBroadcastFullMode
                });

            System.Threading.Channels.Channel<Indicators> indicatorsBroadcastChannel = System.Threading.Channels.Channel.CreateBounded<Indicators>(
                new System.Threading.Channels.BoundedChannelOptions(indicatorsBroadcastCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    AllowSynchronousContinuations = false,
                    FullMode = indicatorsBroadcastFullMode
                });

            System.Threading.Channels.Channel<Market.Strategy.Signal> signalBroadcastChannel = System.Threading.Channels.Channel.CreateBounded<Market.Strategy.Signal>(
                new System.Threading.Channels.BoundedChannelOptions(signalBroadcastCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    AllowSynchronousContinuations = false,
                    FullMode = signalBroadcastFullMode
                });

            return new StrategyEventChannels
            {
                Klines = klineChannel,
                Trades = tradeChannel,
                KlineBroadcasts = klineBroadcastChannel,
                TradeBroadcasts = tradeBroadcastChannel,
                IndicatorsBroadcasts = indicatorsBroadcastChannel,
                SignalBroadcasts = signalBroadcastChannel
            };
        }
    }
}
