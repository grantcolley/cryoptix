namespace Cryoptix.Strategy.Status
{
    public class StrategyStateStore
    {
        private StrategyStatus _status = new() { StrategyState = StrategyState.Idle };

        public StrategyStatus Get() => Volatile.Read(ref _status);
        public void Set(StrategyStatus status) => Volatile.Write(ref _status, status);
    }
}
