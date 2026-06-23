using System.Threading.Channels;

namespace Cryoptix.Strategy.Channel
{
    /// <summary>
    /// Represents the strategy channel options.
    /// </summary>
    public sealed class StrategyChannelOptions
    {
        /// <summary>
        /// Gets or sets the kline capacity.
        /// </summary>
        public int KlineCapacity { get; set; }
        /// <summary>
        /// Gets or sets the trade capacity.
        /// </summary>
        public int TradeCapacity { get; set; }
        /// <summary>
        /// Gets or sets the trade full mode.
        /// </summary>
        public BoundedChannelFullMode TradeFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the kline full mode.
        /// </summary>
        public BoundedChannelFullMode KlineFullMode { get; set; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the kline broadcast capacity.
        /// </summary>
        public int KlineBroadcastCapacity { get; init; }
        /// <summary>
        /// Gets or sets the trade broadcast capacity.
        /// </summary>
        public int TradeBroadcastCapacity { get; init; }
        /// <summary>
        /// Gets or sets the kline broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode KlineBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the trade broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode TradeBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the indicators broadcast capacity.
        /// </summary>
        public int IndicatorsBroadcastCapacity { get; init; }
        /// <summary>
        /// Gets or sets the signal broadcast capacity.
        /// </summary>
        public int SignalBroadcastCapacity { get; init; }
        /// <summary>
        /// Gets or sets the indicators broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode IndicatorsBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
        /// <summary>
        /// Gets or sets the signal broadcast full mode.
        /// </summary>
        public BoundedChannelFullMode SignalBroadcastFullMode { get; init; } = BoundedChannelFullMode.DropOldest;
    }
}
