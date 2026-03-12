using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Execution;

namespace Cryoptix.Web.API.Services
{
    public class StrategyBackgroundService(
        IStrategyCommandQueue strategyCommandQueue,
        IStrategyExecution strategyExecutionService,
        ILogger<StrategyBackgroundService> logger) : BackgroundService
    {
        private readonly IStrategyCommandQueue _strategyCommandQueue = strategyCommandQueue;
        private readonly IStrategyExecution _strategyExecutionService = strategyExecutionService;
        private readonly ILogger<StrategyBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
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

                            await _strategyExecutionService.StartAsync(strategyCommand.Strategy, cancellationToken);
                            break;

                        case StrategyCommandType.Update:
                            if (strategyCommand.Strategy is null)
                            {
                                _logger.LogWarning("Received Update command without strategy payload.");
                                break;
                            }

                            await _strategyExecutionService.UpdateAsync(strategyCommand.Strategy);
                            break;

                        case StrategyCommandType.Stop:
                            await _strategyExecutionService.StopAsync();
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.StopAsync(cancellationToken);
                await _strategyExecutionService.StopAsync();
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
