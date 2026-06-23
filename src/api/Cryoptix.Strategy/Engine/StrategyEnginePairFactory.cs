namespace Cryoptix.Strategy.Engine
{
    /// <summary>
    /// Represents the strategy engine pair factory.
    /// </summary>
    public sealed class StrategyEnginePairFactory(
        IEnumerable<IStrategyEnginePair> enginePairs) : IStrategyEnginePairFactory
    {
        private readonly Dictionary<StrategyEngineType, IStrategyEnginePair> _enginePairs =
            enginePairs.ToDictionary(x => x.StrategyEngineType);

        /// <summary>
        /// Executes the get operation.
        /// </summary>
        /// <param name="strategyEngineType">The strategy engine type value.</param>
        /// <returns>The get result.</returns>
        public IStrategyEnginePair Get(StrategyEngineType strategyEngineType)
        {
            if (!_enginePairs.TryGetValue(strategyEngineType, out IStrategyEnginePair? pair))
            {
                throw new InvalidOperationException(
                    $"No strategy engine pair registered for strategy type '{strategyEngineType}'.");
            }

            return pair;
        }
    }
}
