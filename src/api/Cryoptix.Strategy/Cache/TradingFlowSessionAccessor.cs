using Cryoptix.Strategy.Processor;

namespace Cryoptix.Strategy.Cache
{
    public sealed class TradingFlowSessionAccessor : ITradingFlowSessionAccessor
    {
        private readonly object _gate = new();
        private StrategyProcessorSession? _current;

        public void SetCurrent(StrategyProcessorSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            lock (_gate)
            {
                _current = session;
            }
        }

        public void ClearCurrent()
        {
            lock (_gate)
            {
                _current = null;
            }
        }

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
