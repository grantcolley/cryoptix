using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Catalog
{
    /// <summary>
    /// Defines the i strategy processor catalog contract.
    /// </summary>
    public interface IStrategyProcessorCatalog
    {
        bool TryCreate(StrategyProcessorType key, out Func<IStrategyProcessor> strategyProcessorFactory);
        IReadOnlyCollection<StrategyProcessorType> Keys { get; }
    }
}
