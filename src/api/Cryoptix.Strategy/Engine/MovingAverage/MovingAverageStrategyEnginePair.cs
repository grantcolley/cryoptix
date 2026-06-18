namespace Cryoptix.Strategy.Engine.MovingAverage
{
    public sealed class MovingAverageStrategyEnginePair(
        MovingAverageIndicatorEngine indicatorEngine,
        MovingAverageSignalEngine signalEngine) : IStrategyEnginePair
    {
        public StrategyEngineType StrategyEngineType => StrategyEngineType.MovingAverage;

        public IStrategyIndicatorEngine IndicatorEngine { get; } = indicatorEngine;
        public IStrategySignalEngine SignalEngine { get; } = signalEngine;
    }
}
