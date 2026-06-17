namespace Cryoptix.Strategy.Engine
{
    public interface IStrategyEnginePair
    {
        StrategyEngineType StrategyEngineType { get; }
        IStrategyIndicatorEngine IndicatorEngine { get; }
        IStrategySignalEngine SignalEngine { get; }
    }
}
