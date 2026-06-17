namespace Cryoptix.Strategy.Engine
{
    public interface IStrategyEnginePairFactory
    {
        IStrategyEnginePair Get(StrategyEngineType strategyEngineType);
    }
}
