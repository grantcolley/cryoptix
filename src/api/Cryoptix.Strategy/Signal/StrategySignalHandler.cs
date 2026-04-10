using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Signal
{
    public sealed class StrategySignalHandler(ILogger<StrategySignalHandler> logger) : IStrategySignalHandler
    {
        private readonly ILogger<StrategySignalHandler> _logger = logger;

        public Task HandleAsync(
            StrategyAnalysisContext context,
            SignalEvaluationResult signal,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(signal);

            cancellationToken.ThrowIfCancellationRequested();

            if (signal.Signal == StrategySignal.None)
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation(
                "Signal generated for {Symbol} [{StrategyType}]: {Signal}. Reason: {Reason}",
                context.Strategy.Symbol,
                context.Strategy.StrategyProcessorType,
                signal.Signal,
                signal.Reason);

            return Task.CompletedTask;
        }
    }
}
