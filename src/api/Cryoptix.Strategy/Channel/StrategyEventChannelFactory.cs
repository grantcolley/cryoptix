using Cryoptix.Strategy.Event;
using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Channel
{
    public sealed class StrategyEventChannelFactory(
        StrategyChannelOptions options) : IStrategyEventChannelFactory
    {
        private readonly StrategyChannelOptions _options = options;

        public StrategyEventChannels Create()
        {
            return Create(_options.DropTradesWhenFull, _options.KlineCapacity, _options.KlineFullMode, _options.TradeCapacity, _options.KlineBroadcastCapacity, _options.KlineBroadcastFullMode, _options.TradeBroadcastCapacity, _options.TradeBroadcastFullMode);
        }

        public StrategyEventChannels Create(Strategies.Strategy strategy)
        {
            bool dropTradesWhenFull = strategy.SubscriptionChannelDropTradesWhenFull;
            int klineCapacity = strategy.SubscriptionChannelKlineCapacity > 0 ? strategy.SubscriptionChannelKlineCapacity : _options.KlineCapacity;
            System.Threading.Channels.BoundedChannelFullMode klineFullMode = strategy.SubscriptionChannelKlineFullMode;
            int tradeCapacity = strategy.SubscriptionChannelTradeCapacity > 0 ? strategy.SubscriptionChannelTradeCapacity : _options.TradeCapacity;
            int klineBroadcastCapacity = strategy.KlineBroadcastCapacity > 0 ? strategy.KlineBroadcastCapacity : _options.KlineBroadcastCapacity;
            System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode = strategy.KlineBroadcastFullMode;
            int tradeBroadcastCapacity = strategy.TradeBroadcastCapacity > 0 ? strategy.TradeBroadcastCapacity : _options.TradeBroadcastCapacity;
            System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode = strategy.TradeBroadcastFullMode;

            return Create(dropTradesWhenFull, klineCapacity, klineFullMode, tradeCapacity, klineBroadcastCapacity, klineBroadcastFullMode, tradeBroadcastCapacity, tradeBroadcastFullMode);
        }

        public StrategyEventChannels Create(bool dropTradesWhenFull, int klineCapacity, System.Threading.Channels.BoundedChannelFullMode klineFullMode, int tradeCapacity, int klineBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode, int tradeBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode)
        {
            System.Threading.Channels.BoundedChannelFullMode tradeFullMode = dropTradesWhenFull
                ? System.Threading.Channels.BoundedChannelFullMode.DropOldest
                : System.Threading.Channels.BoundedChannelFullMode.Wait;

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

            return new StrategyEventChannels
            {
                Klines = klineChannel,
                Trades = tradeChannel,
                KlineBroadcasts = klineBroadcastChannel,
                TradeBroadcasts = tradeBroadcastChannel
            };
        }
    }
}
