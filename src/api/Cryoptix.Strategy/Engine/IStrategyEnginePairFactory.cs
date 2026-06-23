namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Defines the i strategy engine pair factory contract.
    /// </summary>
    public interface IStrategyEnginePairFactory
    {
        IStrategyEnginePair Get(StrategyEngineType strategyEngineType);
    }
}
