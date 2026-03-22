using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Execution;
using Cryoptix.Strategy.Runtime;

namespace Cryoptix.Strategy.Strategies
{
    public class MovingAverage : IStrategyExecutable
    {
        public readonly StrategyType Type = StrategyType.MovingAverage;

        private ExchangeApi? _exchangeApi;
        private Runtime.Strategy? _strategy;

        public async Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategyRuntime);
            ArgumentNullException.ThrowIfNull(strategyRuntime.ExchangeApi);
            if (strategyRuntime.GetStrategy is null) throw new ArgumentNullException($"{nameof(strategyRuntime)}.GetStrategy()");

            _exchangeApi = strategyRuntime.ExchangeApi;

            // Apply initial config/state
            await ApplyStrategyAsync(strategyRuntime.GetStrategy(), cancellationToken);

            // Start the real-time processing once
            Task strategyExecutionTask = ExecuteStrategyAsync(cancellationToken);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyRuntime.WaitForStrategyUpdateAsync(cancellationToken);

                    Task completed = await Task.WhenAny(strategyUpdateTask, strategyExecutionTask);

                    if (completed == strategyExecutionTask)
                    {
                        // Subscription ended or failed
                        await strategyExecutionTask; // propagate exceptions
                        break;
                    }

                    Runtime.Strategy? updated = strategyRuntime.GetStrategy();
                    await ApplyStrategyAsync(updated, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // normal shutdown
            }

            await strategyExecutionTask;
        }

        private async Task ExecuteStrategyAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exchangeApi?.SubscriptionsApi == null)
            {
                Console.Error.WriteLine("Exchange API does not support subscriptions.");
                return;
            }

            Runtime.Strategy? intitalStrategy = Volatile.Read(ref _strategy);

            if(intitalStrategy == null 
                || string.IsNullOrWhiteSpace(intitalStrategy.Symbol))
            {
                return;
            }

            await using var subscription = await _exchangeApi.SubscriptionsApi.SubscribeToTradesAsync(
                symbol: intitalStrategy.Symbol,
                callback: async trade =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Runtime.Strategy? strategy = Volatile.Read(ref _strategy);
                    if (strategy == null)
                        return;

                    System.Diagnostics.Debug.WriteLine($"{strategy.Name} {strategy.Description}");

                    foreach (var t in trade.Trades)
                    {
                        System.Diagnostics.Debug.WriteLine($"{t.Time} {t.Price} {t.QuoteQuantity}");
                    }
                    // Process the trade and make trading decisions
                },
                onError: ex =>
                {
                    // Handle subscription errors
                    Console.Error.WriteLine($"Subscription error: {ex}");
                },
                cancellationToken: cancellationToken);

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // normal shutdown
            }
        }

        private Task ApplyStrategyAsync(Runtime.Strategy? strategy, CancellationToken cancellationToken)
        {
            Volatile.Write(ref _strategy, strategy);
            return Task.CompletedTask;
        }
    }
}
