namespace Cryoptix.Strategy.Execution
{
    public sealed class AsyncAutoResetEvent
    {
        private static readonly Task Completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waits = new();
        private bool _signaled;

        public Task WaitAsync(CancellationToken cancellationToken = default)
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return Completed;
                }

                TaskCompletionSource<bool> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
                }

                _waits.Enqueue(taskCompletionSource);
                return taskCompletionSource.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<bool>? toRelease = null;

            lock (_waits)
            {
                while (_waits.Count > 0)
                {
                    toRelease = _waits.Dequeue();

                    if (!toRelease.Task.IsCompleted)
                    {
                        break;
                    }

                    toRelease = null;
                }

                if (toRelease is null)
                {
                    _signaled = true;
                    return;
                }
            }

            toRelease.TrySetResult(true);
        }
    }
}
