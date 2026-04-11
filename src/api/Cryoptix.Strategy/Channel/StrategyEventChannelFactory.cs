using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Channel
{
    public sealed class StrategyEventChannelFactory(
        StrategyChannelOptions options) : IStrategyEventChannelFactory
    {
        private readonly StrategyChannelOptions _options = options;

        public StrategyEventChannels Create()
        {
            BoundedChannelFullMode tradeFullMode = _options.DropTradesWhenFull
                ? BoundedChannelFullMode.DropOldest
                : BoundedChannelFullMode.Wait;

            Channel<KlineMarketEvent> klineChannel = System.Threading.Channels.Channel.CreateBounded<KlineMarketEvent>(
                new BoundedChannelOptions(_options.KlineCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                    FullMode = _options.KlineFullMode
                });

            Channel<TradeMarketEvent> tradeChannel = System.Threading.Channels.Channel.CreateBounded<TradeMarketEvent>(
                new BoundedChannelOptions(_options.TradeCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                    FullMode = tradeFullMode
                });

            return new StrategyEventChannels
            {
                Klines = klineChannel,
                Trades = tradeChannel
            };
        }
    }
}
