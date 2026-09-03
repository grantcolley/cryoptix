namespace Cryoptix.Strategy.Channel
{
    /// <summary>
    /// Defines the strategy event channel factory contract.
    /// </summary>
    public interface IStrategyEventChannelFactory
    {
        /// <summary>
        /// Creates the operation.
        /// </summary>
        /// <returns>The create result.</returns>
        StrategyEventChannels Create();
        /// <summary>
        /// Creates the operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <returns>The create result.</returns>
        StrategyEventChannels Create(Strategies.Strategy strategy);
        /// <summary>
        /// Creates the operation.
        /// </summary>
        /// <param name="klineCapacity">The kline capacity.</param>
        /// <param name="klineFullMode">The kline full mode.</param>
        /// <param name="tradeCapacity">The trade capacity.</param>
        /// <param name="tradeFullMode">The trade full mode.</param>
        /// <param name="marketEventDispatcherCapacity">The market event dispatcher capacity.</param>
        /// <param name="marketEventDispatcherFullMode">The market event dispatcher full mode.</param>
        /// <param name="klineBroadcastCapacity">The kline broadcast capacity.</param>
        /// <param name="klineBroadcastFullMode">The kline broadcast full mode.</param>
        /// <param name="tradeBroadcastCapacity">The trade broadcast capacity.</param>
        /// <param name="tradeBroadcastFullMode">The trade broadcast full mode.</param>
        /// <param name="indicatorsBroadcastCapacity">The indicators broadcast capacity.</param>
        /// <param name="indicatorsBroadcastFullMode">The indicators broadcast full mode.</param>
        /// <param name="signalBroadcastCapacity">The signal broadcast capacity.</param>
        /// <param name="signalBroadcastFullMode">The signal broadcast full mode.</param>
        /// <param name="broadcastQueueCapacity">The broadcast queue capacity value.</param>
        /// <param name="broadcastQueueFullMode">The broadcast queue full mode value.</param>
        /// <returns>The create result.</returns>
        StrategyEventChannels Create(
            int klineCapacity,
            System.Threading.Channels.BoundedChannelFullMode klineFullMode,
            int tradeCapacity, 
            System.Threading.Channels.BoundedChannelFullMode tradeFullMode,
            int marketEventDispatcherCapacity,
            System.Threading.Channels.BoundedChannelFullMode marketEventDispatcherFullMode,
            int klineBroadcastCapacity, 
            System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode, 
            int tradeBroadcastCapacity,
            System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode,
            int indicatorsBroadcastCapacity,
            System.Threading.Channels.BoundedChannelFullMode indicatorsBroadcastFullMode,
            int signalBroadcastCapacity,
            System.Threading.Channels.BoundedChannelFullMode signalBroadcastFullMode,
            int broadcastQueueCapacity,
            System.Threading.Channels.BoundedChannelFullMode broadcastQueueFullMode);
    }
}
