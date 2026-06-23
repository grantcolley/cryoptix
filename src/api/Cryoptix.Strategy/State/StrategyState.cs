namespace Cryoptix.Strategy.State
{
    /// <summary>
    /// Defines the strategy state values.
    /// </summary>
    public enum StrategyState
    {
        /// <summary>
        /// Specifies the idle value.
        /// </summary>
        Idle,
        /// <summary>
        /// Specifies the starting value.
        /// </summary>
        Starting,
        /// <summary>
        /// Specifies the running value.
        /// </summary>
        Running,
        /// <summary>
        /// Specifies the stopping value.
        /// </summary>
        Stopping,
        /// <summary>
        /// Specifies the faulted value.
        /// </summary>
        Faulted,
    }
}
