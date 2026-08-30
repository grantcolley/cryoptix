using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Represents the market event session accessor.
    /// </summary>
    public sealed class MarketEventSessionAccessor : IMarketEventSessionAccessor
    {
        private readonly Lock _gate = new();
        private StrategyProcessorSession? _current;

        /// <summary>
        /// Executes the set current operation.
        /// </summary>
        /// <param name="session">The session value.</param>
        public void SetCurrent(StrategyProcessorSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            lock (_gate)
            {
                _current = session;
            }
        }

        /// <summary>
        /// Executes the clear current operation.
        /// </summary>
        public void ClearCurrent()
        {
            lock (_gate)
            {
                _current = null;
            }
        }

        /// <summary>
        /// Executes the try get current operation.
        /// </summary>
        /// <param name="session">The session value.</param>
        /// <returns>The try get current result.</returns>
        public bool TryGetCurrent(out StrategyProcessorSession? session)
        {
            lock (_gate)
            {
                session = _current;
                return session != null;
            }
        }
    }
}
