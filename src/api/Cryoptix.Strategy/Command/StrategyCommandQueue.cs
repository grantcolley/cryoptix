using System.Threading.Channels;

namespace Cryoptix.Strategy.Command
{
    public sealed class StrategyCommandQueue(Channel<StrategyCommand> channel) : IStrategyCommandQueue
    {
        public ValueTask EnqueueAsync(StrategyCommand command, CancellationToken ct)
            => channel.Writer.WriteAsync(command, ct);

        public IAsyncEnumerable<StrategyCommand> ReadAllAsync(CancellationToken ct)
            => channel.Reader.ReadAllAsync(ct);
    }
}
