namespace Cryoptix.Strategy.Subscriptions
{
    public sealed class SubscriptionSession(
        IAsyncDisposable subscriptionHandle,
        CancellationTokenSource cts,
        Task completion) : IAsyncDisposable
    {
        private readonly IAsyncDisposable _subscriptionHandle = subscriptionHandle;
        private readonly CancellationTokenSource _cancellationTokenSource = cts;
        private int _disposed;

        public Task Completion { get; } = completion;

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
