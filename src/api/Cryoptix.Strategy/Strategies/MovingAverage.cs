using Cryoptix.Strategy.Execution;
using Cryoptix.Strategy.Runtime;

namespace Cryoptix.Strategy.Strategies
{
    public class MovingAverage : IStrategyExecutable
    {
        public readonly StrategyType Type = StrategyType.MovingAverage;

        public async Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategyRuntime);
            if (strategyRuntime.GetStrategy == null) throw new ArgumentNullException(nameof(strategyRuntime.GetStrategy));

            Runtime.Strategy? current = strategyRuntime.GetStrategy();
            await ApplyStrategyAsync(current, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                Task startegyExecutionTask = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                Task strategyUpdateTask = strategyRuntime.WaitForStrategyUpdateAsync(cancellationToken);

                Task completed = await Task.WhenAny(startegyExecutionTask, strategyUpdateTask);

                if (completed == strategyUpdateTask)
                {
                    current = strategyRuntime.GetStrategy();
                    await ApplyStrategyAsync(current, cancellationToken);
                    continue;
                }

                await ExecuteStrategyAsync(current, cancellationToken);
            }
        }

        private Task ExecuteStrategyAsync(Runtime.Strategy? strategy, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task ApplyStrategyAsync(Runtime.Strategy? strategy, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
