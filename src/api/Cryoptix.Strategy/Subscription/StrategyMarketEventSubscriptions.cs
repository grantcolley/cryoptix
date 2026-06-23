namespace Cryoptix.Strategy.Subscription
{
    /// <summary>
    /// Represents the strategy market event subscriptions.
    /// </summary>
    public sealed class StrategyMarketEventSubscriptions(
        IAsyncDisposable subscriptionHandle,
        CancellationTokenSource cts,
        Task completion) : IAsyncDisposable
    {
        private readonly IAsyncDisposable _subscriptionHandle = subscriptionHandle;
        private readonly CancellationTokenSource _cancellationTokenSource = cts;
        private int _disposed;

        /// <summary>
        /// Gets the completion.
        /// </summary>
        public Task Completion { get; } = completion;

        /// <summary>
        /// Executes the dispose async operation.
        /// </summary>
        /// <returns>The dispose async result.</returns>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch
            {
            }

            try
            {
                await _subscriptionHandle.DisposeAsync();
            }
            finally
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}
