namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Defines the i strategy engine pair contract.
    /// </summary>
    public interface IStrategyEnginePair
    {
        StrategyEngineType StrategyEngineType { get; }
        IStrategyIndicatorEngine IndicatorEngine { get; }
        IStrategySignalEngine SignalEngine { get; }
    }
}
