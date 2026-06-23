using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Catalog
{
    /// <summary>
    /// Represents the strategy processor catalog.
    /// </summary>
    public sealed class StrategyProcessorCatalog(IEnumerable<KeyValuePair<StrategyProcessorType, Func<IStrategyProcessor>>> strategyProcessors) : IStrategyProcessorCatalog
    {
        private readonly Dictionary<StrategyProcessorType, Func<IStrategyProcessor>> _strategyProcessorMap = strategyProcessors.ToDictionary(e => e.Key, e => e.Value);

        /// <summary>
        /// Executes the try create operation.
        /// </summary>
        /// <param name="key">The key value.</param>
        /// <param name="strategyProcessorFactory">The strategy processor factory value.</param>
        /// <returns>The try create result.</returns>
        public bool TryCreate(StrategyProcessorType key, out Func<IStrategyProcessor> strategyProcessorFactory) => _strategyProcessorMap.TryGetValue(key, out strategyProcessorFactory!);

        /// <summary>
        /// Gets or sets the keys.
        /// </summary>
        public IReadOnlyCollection<StrategyProcessorType> Keys => [.. _strategyProcessorMap.Keys];
    }
}
