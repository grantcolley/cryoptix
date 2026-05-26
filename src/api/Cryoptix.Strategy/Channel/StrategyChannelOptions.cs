using System.Threading.Channels;

namespace Cryoptix.Strategy.Channel
{
    public sealed class StrategyChannelOptions
    {
        public int KlineCapacity { get; set; }
        public int TradeCapacity { get; set; }
        public BoundedChannelFullMode TradeFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode KlineFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        public int KlineBroadcastCapacity { get; init; }
        public int TradeBroadcastCapacity { get; init; }
        public BoundedChannelFullMode KlineBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode TradeBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public int IndicatorsBroadcastCapacity { get; init; }
        public int SignalBroadcastCapacity { get; init; }
        public BoundedChannelFullMode IndicatorsBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode SignalBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
    }
}
