namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Defines the strategy engine pair contract.
    /// </summary>
    public interface IStrategyEnginePair
    {
        /// <summary>
        /// Gets the strategy engine type.
        /// </summary>
        StrategyEngineType StrategyEngineType { get; }
        /// <summary>
        /// Gets the indicator engine.
        /// </summary>
        IStrategyIndicatorEngine IndicatorEngine { get; }
        /// <summary>
        /// Gets the signal engine.
        /// </summary>
        IStrategySignalEngine SignalEngine { get; }
    }
}
