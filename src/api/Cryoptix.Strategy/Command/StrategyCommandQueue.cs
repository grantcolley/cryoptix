using System.Threading.Channels;

namespace Cryoptix.Strategy.Command
{
    /// <summary>
    /// Represents the strategy command queue.
    /// </summary>
    public sealed class StrategyCommandQueue(Channel<StrategyCommand> channel) : IStrategyCommandQueue
    {
        /// <summary>
        /// Executes the enqueue async operation.
        /// </summary>
        /// <param name="command">The command value.</param>
        /// <param name="ct">The ct value.</param>
        /// <returns>The enqueue async result.</returns>
        public ValueTask EnqueueAsync(StrategyCommand command, CancellationToken ct)
            => channel.Writer.WriteAsync(command, ct);

        /// <summary>
        /// Executes the read all async operation.
        /// </summary>
        /// <param name="ct">The ct value.</param>
        /// <returns>The read all async result.</returns>
        public IAsyncEnumerable<StrategyCommand> ReadAllAsync(CancellationToken ct)
            => channel.Reader.ReadAllAsync(ct);
    }
}
