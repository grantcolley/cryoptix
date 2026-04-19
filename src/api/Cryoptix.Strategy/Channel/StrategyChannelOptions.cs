using System.Threading.Channels;

namespace Cryoptix.Strategy.Channel
{
    public sealed class StrategyChannelOptions
    {
        public int KlineCapacity { get; set; }
        public int TradeCapacity { get; set; }

        /// <summary>
        /// When true, excess trades are dropped instead of blocking writers.
        /// Useful when trade volume spikes and only recent trades matter.
        /// </summary>
        public bool DropTradesWhenFull { get; set; } = true;
        public BoundedChannelFullMode KlineFullMode { get; set; } = BoundedChannelFullMode.Wait;

        public int KlineBroadcastCapacity { get; init; } = 512;
        public int TradeBroadcastCapacity { get; init; } = 5_000;
        public BoundedChannelFullMode KlineBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        public BoundedChannelFullMode TradeBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
    }
}
