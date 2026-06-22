using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Command;

namespace Cryoptix.Web.API.Services
{
    internal class StrategyBackgroundService(
        IStrategyCommandQueue strategyCommandQueue,
        IStrategyAgent strategyAgent,
        ILogger<StrategyBackgroundService> logger) : BackgroundService
    {
        private readonly IStrategyCommandQueue _strategyCommandQueue = strategyCommandQueue;
        private readonly IStrategyAgent _strategyAgent = strategyAgent;
        private readonly ILogger<StrategyBackgroundService> _logger = logger;

        /// <summary>
        /// Executes the execute async operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The execute async result.</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            _logger.LogInformation("Strategy background service started.");

            await foreach (StrategyCommand strategyCommand in _strategyCommandQueue.ReadAllAsync(cancellationToken))
            {
                try
                {
                    switch (strategyCommand.StrategyCommandType)
                    {
                        case StrategyCommandType.Start:
                            if (strategyCommand.Strategy is null)
                            {
                                _logger.LogWarning("Received Start command without strategy payload.");
                                break;
                            }

                            await _strategyAgent.StartAsync(strategyCommand.Strategy, cancellationToken);
                            break;

                        case StrategyCommandType.Update:
                            if (strategyCommand.Strategy is null)
                            {
                                _logger.LogWarning("Received Update command without strategy payload.");
                                break;
                            }

                            await _strategyAgent.UpdateAsync(strategyCommand.Strategy);
                            break;

                        case StrategyCommandType.Stop:
                            await _strategyAgent.StopAsync();
                            break;

                        default:
                            _logger.LogWarning(
                                "Received unknown strategy command type {StrategyCommandType}",
                                strategyCommand.StrategyCommandType);
                            break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Strategy background service is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing strategy command {StrategyCommandType} for strategy {StrategyName}",
                        strategyCommand.StrategyCommandType,
                        strategyCommand.Strategy?.Name);
                }
            }
        }

        /// <summary>
        /// Executes the stop async operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token value.</param>
        /// <returns>The stop async result.</returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.StopAsync(cancellationToken);
                await _strategyAgent.StopAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Strategy background service is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping strategy background service");
            }
        }
    }
}
