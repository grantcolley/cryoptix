namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Defines the strategy engine pair factory contract.
    /// </summary>
    public interface IStrategyEnginePairFactory
    {
        /// <summary>
        /// Gets the operation.
        /// </summary>
        /// <param name="strategyEngineType">The strategy engine type.</param>
        /// <returns>The get result.</returns>
        IStrategyEnginePair Get(StrategyEngineType strategyEngineType);
    }
}
