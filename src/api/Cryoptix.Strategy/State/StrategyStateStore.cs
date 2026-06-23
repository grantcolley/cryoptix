using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.State
{
    /// <summary>
    /// Represents the strategy state store.
    /// </summary>
    public sealed class StrategyStateStore(ILogger<StrategyStateStore> logger)
    {
        private readonly ILogger<StrategyStateStore> _logger = logger;

        private StrategyStatus _status = new() { StrategyState = StrategyState.Idle };

        /// <summary>
        /// Executes the get operation.
        /// </summary>
        /// <returns>The get result.</returns>
        public StrategyStatus Get() => Volatile.Read(ref _status);

        /// <summary>
        /// Executes the set operation.
        /// </summary>
        /// <param name="status">The status value.</param>
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
