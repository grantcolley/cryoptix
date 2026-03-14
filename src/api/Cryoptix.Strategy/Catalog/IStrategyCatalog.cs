using Cryoptix.Strategy.Execution;
using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Catalog
{
    public interface IStrategyCatalog
    {
        bool TryCreate(StrategyType key, out Func<IStrategyExecutable> factory);
        IReadOnlyCollection<StrategyType> Keys { get; }
    }
}
