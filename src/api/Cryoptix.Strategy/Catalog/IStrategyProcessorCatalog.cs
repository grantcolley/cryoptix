using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Catalog
{
    public interface IStrategyProcessorCatalog
    {
        bool TryCreate(StrategyProcessorType key, out Func<IStrategyProcessor> strategyProcessorFactory);
        IReadOnlyCollection<StrategyProcessorType> Keys { get; }
    }
}
