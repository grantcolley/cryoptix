namespace Cryoptix.Strategy.Processor
{
    public sealed class CompositeAsyncDisposable : IAsyncDisposable
    {
        private readonly IAsyncDisposable[] _inner;
        private int _disposed;

        public CompositeAsyncDisposable(params IAsyncDisposable[] inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            List<Exception>? exceptions = null;

            for (int i = _inner.Length - 1; i >= 0; i--)
            {
                try
                {
                    await _inner[i].DisposeAsync();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            if (exceptions is { Count: > 0 })
                throw new AggregateException(exceptions);
        }
    }
}
