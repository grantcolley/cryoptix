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
            System.Threading.Channels.BoundedChannelFullMode tradeFullMode = _options.DropTradesWhenFull
                ? System.Threading.Channels.BoundedChannelFullMode.DropOldest
                : System.Threading.Channels.BoundedChannelFullMode.Wait;

            System.Threading.Channels.Channel<KlineMarketEvent> klineChannel = System.Threading.Channels.Channel.CreateBounded<KlineMarketEvent>(
                new System.Threading.Channels.BoundedChannelOptions(_options.KlineCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                    FullMode = _options.KlineFullMode
                });

            System.Threading.Channels.Channel<TradeMarketEvent> tradeChannel = System.Threading.Channels.Channel.CreateBounded<TradeMarketEvent>(
                new System.Threading.Channels.BoundedChannelOptions(_options.TradeCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                    FullMode = tradeFullMode
                });

            System.Threading.Channels.Channel<Kline> klineBroadcastChannel = System.Threading.Channels.Channel.CreateBounded<Kline>(
                new System.Threading.Channels.BoundedChannelOptions(_options.KlineBroadcastCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    AllowSynchronousContinuations = false,
                    FullMode = _options.KlineBroadcastFullMode
                });

            System.Threading.Channels.Channel<Trade> tradeBroadcastChannel = System.Threading.Channels.Channel.CreateBounded<Trade>(
                new System.Threading.Channels.BoundedChannelOptions(_options.TradeBroadcastCapacity)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    AllowSynchronousContinuations = false,
                    FullMode = _options.TradeBroadcastFullMode
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
