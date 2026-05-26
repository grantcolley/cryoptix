using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Channel
{
    public sealed class StrategyEventChannels
    {
        public required Channel<KlineMarketEvent> Klines { get; init; }
        public required Channel<TradeMarketEvent> Trades { get; init; }
        public required Channel<Kline> KlineBroadcasts { get; init; }
        public required Channel<Trade> TradeBroadcasts { get; init; }
        public required Channel<Indicators> IndicatorsBroadcasts { get; init; }
        public required Channel<Market.Strategy.Signal> SignalBroadcasts { get; init; }

        public void CompleteWriters(Exception? error = null)
        {
            Klines.Writer.TryComplete(error);
            Trades.Writer.TryComplete(error);
            KlineBroadcasts.Writer.TryComplete(error);
            TradeBroadcasts.Writer.TryComplete(error);
            IndicatorsBroadcasts.Writer.TryComplete(error);
            SignalBroadcasts.Writer.TryComplete(error);
        }
    }
}
