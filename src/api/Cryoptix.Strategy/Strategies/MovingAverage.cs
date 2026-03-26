using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Execution;
using Cryoptix.Strategy.Runtime;
using Cryoptix.Strategy.Subscriptions;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Strategies
{
    public class MovingAverage(ILogger<MovingAverage> logger) : IStrategyExecutable
    {
        public readonly StrategyType Type = StrategyType.MovingAverage;

        private readonly ILogger<MovingAverage> _logger = logger;

        private ExchangeApi? _exchangeApi;
        private Runtime.Strategy? _strategy;

        public async Task ExecuteAsync(StrategyRuntime strategyRuntime, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(strategyRuntime);
            ArgumentNullException.ThrowIfNull(strategyRuntime.ExchangeApi);
            if (strategyRuntime.GetStrategy is null) throw new ArgumentNullException($"{nameof(strategyRuntime)}.GetStrategy()");

            _exchangeApi = strategyRuntime.ExchangeApi;

            await ApplyStrategyAsync(strategyRuntime.GetStrategy(), cancellationToken);

            SubscriptionSession subscriptionSession = await StartStrategySubscriptionsAsync(cancellationToken);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Task strategyUpdateTask = strategyRuntime.WaitForStrategyUpdateAsync(cancellationToken);
                    Task sessionCompletionTask = subscriptionSession.Completion;

                    Task completed = await Task.WhenAny(strategyUpdateTask, sessionCompletionTask);

                    if (completed == sessionCompletionTask)
                    {
                        // Propagate failure/cancellation from the subscription lifetime task.
                        await sessionCompletionTask;
                        break;
                    }

                    Runtime.Strategy? updated = strategyRuntime.GetStrategy();
                    await ApplyStrategyAsync(updated, cancellationToken);
                }
            }
            finally
            {
                await subscriptionSession.DisposeAsync();
            }
        }

        private async Task<SubscriptionSession> StartStrategySubscriptionsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exchangeApi?.SubscriptionsApi == null)
            {
                throw new InvalidOperationException("Exchange API does not support subscriptions.");
            }

            Runtime.Strategy? intitalStrategy = Volatile.Read(ref _strategy);

            if(intitalStrategy == null 
                || string.IsNullOrWhiteSpace(intitalStrategy.Symbol))
            {
                throw new InvalidOperationException("Strategy symbol is required.");
            }

            CancellationTokenSource sessionCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                IAsyncDisposable subscriptionHandle = await _exchangeApi.SubscriptionsApi.SubscribeToTradesAsync(
                    symbol: intitalStrategy.Symbol,
                    onCallback: trade => OnTrade(trade),
                    onError: ex =>
                    {
                        // Handle subscription errors
                        _logger.LogError($"Subscription error: {ex}");
                    },
                    cancellationToken: sessionCancellationTokenSource.Token);

                Task completionTask = WaitUntilCancelledAsync(sessionCancellationTokenSource.Token);

                return new SubscriptionSession(subscriptionHandle, sessionCancellationTokenSource, completionTask);
            }
            catch
            {
                sessionCancellationTokenSource.Dispose();
                throw;
            }
        }

        private Task ApplyStrategyAsync(Runtime.Strategy? strategy, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Volatile.Write(ref _strategy, strategy);
            return Task.CompletedTask;
        }

        private void OnTrade(TradeEventArgs trade)
        {
            Runtime.Strategy? strategy = Volatile.Read(ref _strategy);
            if (strategy == null)
                return;

            foreach (Trade t in trade.Trades)
            {
                _logger.LogInformation($"{t.Time} {t.Price} {t.QuoteQuantity}");
            }

            // Process the trade data and make trading decisions
        }

        private static async Task WaitUntilCancelledAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }
}
