using System.Threading.Channels;

namespace Cryoptix.Strategy.Command
{
    public sealed class StrategyCommandQueue : IStrategyCommandQueue
    {
        private readonly Channel<StrategyCommand> _channel;

        public StrategyCommandQueue(Channel<StrategyCommand> channel) => _channel = channel;

        public ValueTask EnqueueAsync(StrategyCommand command, CancellationToken ct)
            => _channel.Writer.WriteAsync(command, ct);

        public IAsyncEnumerable<StrategyCommand> ReadAllAsync(CancellationToken ct)
            => _channel.Reader.ReadAllAsync(ct);
    }
}
