namespace Cryoptix.Strategy.Engine.MovingAverage
{
    /// <summary>
    /// Represents the moving average strategy engine pair.
    /// </summary>
    public sealed class MovingAverageStrategyEnginePair(
        MovingAverageIndicatorEngine indicatorEngine,
        MovingAverageSignalEngine signalEngine) : IStrategyEnginePair
    {
        /// <summary>
        /// Gets or sets the strategy engine type.
        /// </summary>
        public StrategyEngineType StrategyEngineType => StrategyEngineType.MovingAverage;

        /// <summary>
        /// Gets or sets the indicator engine.
        /// </summary>
        public IStrategyIndicatorEngine IndicatorEngine { get; } = indicatorEngine;

        /// <summary>
        /// Gets or sets the signal engine.
        /// </summary>
        public IStrategySignalEngine SignalEngine { get; } = signalEngine;
    }
}
