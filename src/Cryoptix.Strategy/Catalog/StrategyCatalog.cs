using Cryoptix.Strategy.Execution;
using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Catalog
{
    public class StrategyCatalog(IEnumerable<KeyValuePair<StrategyType, Func<IStrategyExecutable>>> entries) : IStrategyCatalog
    {
        private readonly Dictionary<StrategyType, Func<IStrategyExecutable>> _map = entries.ToDictionary(e => e.Key, e => e.Value);

        public bool TryCreate(StrategyType key, out Func<IStrategyExecutable> factory) => _map.TryGetValue(key, out factory!);

        public IReadOnlyCollection<StrategyType> Keys => [.. _map.Keys];
    }
}
