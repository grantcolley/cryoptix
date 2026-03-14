namespace Cryoptix.Exchange.Binance
{
    internal sealed class AsyncSubscription(Func<ValueTask> dispose) : IAsyncDisposable
    {
        private Task? _disposeTask;

        public ValueTask DisposeAsync()
        {
            var existing = Volatile.Read(ref _disposeTask);
            if (existing != null) return new ValueTask(existing);

            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            // If the value of _disposeTask is null assign tcs.Task to it, and return the original value of _disposeTask.
            // So the effect is:
            // - The first thread to call this when _disposeTask is null will successfully set _disposeTask = tcs.Task.
            // - All other threads will do nothing because _disposeTask is no longer null.
            var prior = Interlocked.CompareExchange(ref _disposeTask, tcs.Task, null);
            if (prior != null) return new ValueTask(prior);

            _ = RunDisposeAsync(tcs);

            return new ValueTask(tcs.Task);
        }

        private async Task RunDisposeAsync(TaskCompletionSource<object?> tcs)
        {
            try 
            {
                await dispose().ConfigureAwait(false);
                tcs.TrySetResult(null);
            }
            catch (Exception ex) 
            {
                tcs.TrySetException(ex); 
            }
        }
    }
}
