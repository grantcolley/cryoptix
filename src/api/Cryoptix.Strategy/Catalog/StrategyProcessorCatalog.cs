using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Catalog
{
    public class StrategyProcessorCatalog(IEnumerable<KeyValuePair<StrategyProcessorType, Func<IStrategyProcessor>>> strategyProcessors) : IStrategyProcessorCatalog
    {
        private readonly Dictionary<StrategyProcessorType, Func<IStrategyProcessor>> _strategyProcessorMap = strategyProcessors.ToDictionary(e => e.Key, e => e.Value);

        public bool TryCreate(StrategyProcessorType key, out Func<IStrategyProcessor> strategyProcessorFactory) => _strategyProcessorMap.TryGetValue(key, out strategyProcessorFactory!);

        public IReadOnlyCollection<StrategyProcessorType> Keys => [.. _strategyProcessorMap.Keys];
    }
}
