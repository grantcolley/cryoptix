namespace Cryoptix.Strategy.Engine
{
    public sealed class StrategyEnginePairFactory(
        IEnumerable<IStrategyEnginePair> enginePairs) : IStrategyEnginePairFactory
    {
        private readonly Dictionary<StrategyEngineType, IStrategyEnginePair> _enginePairs =
            enginePairs.ToDictionary(x => x.StrategyEngineType);

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
