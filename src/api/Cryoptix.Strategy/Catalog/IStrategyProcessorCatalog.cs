using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Catalog
{
    /// <summary>
    /// Defines the strategy processor catalog contract.
    /// </summary>
    public interface IStrategyProcessorCatalog
    {
        /// <summary>
        /// Tries to create.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="strategyProcessorFactory">The strategy processor factory.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        bool TryCreate(StrategyProcessorType key, out Func<IStrategyProcessor> strategyProcessorFactory);
        /// <summary>
        /// Gets the keys.
        /// </summary>
        IReadOnlyCollection<StrategyProcessorType> Keys { get; }
    }
}
