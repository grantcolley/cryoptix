using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.State
{
    public sealed class StrategyStateStore(ILogger<StrategyStateStore> logger)
    {
        private readonly ILogger<StrategyStateStore> _logger = logger;

        private StrategyStatus _status = new() { StrategyState = StrategyState.Idle };

        public StrategyStatus Get() => Volatile.Read(ref _status);

        public void Set(StrategyStatus status)
        {
            Volatile.Write(ref _status, status);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Strategy status updated: {@Status}", status);
            }
        }
    }
}
