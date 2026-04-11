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
    }
}
