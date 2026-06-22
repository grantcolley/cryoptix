namespace Cryoptix.Observer.Notification
{
    /// <summary>
    /// Defines the message type values.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Specifies the none value.
        /// </summary>
        None,
        /// <summary>
        /// Specifies the strategy started value.
        /// </summary>
        StrategyStarted,
        /// <summary>
        /// Specifies the strategy updated value.
        /// </summary>
        StrategyUpdated,
        /// <summary>
        /// Specifies the market data snapshot value.
        /// </summary>
        MarketDataSnapshot,
        /// <summary>
        /// Specifies the kline value.
        /// </summary>
        Kline,
        /// <summary>
        /// Specifies the trade value.
        /// </summary>
        Trade,
        /// <summary>
        /// Specifies the indicators value.
        /// </summary>
        Indicators,
        /// <summary>
        /// Specifies the signal value.
        /// </summary>
        Signal
    }
}
