using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Engine
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
