namespace Cryoptix.Strategy.Command
{
    /// <summary>
    /// Defines the strategy command queue contract.
    /// </summary>
    public interface IStrategyCommandQueue
    {
        /// <summary>
        /// Executes the enqueue operation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask EnqueueAsync(StrategyCommand command, CancellationToken ct);
        /// <summary>
        /// Reads the all.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The read all result.</returns>
        IAsyncEnumerable<StrategyCommand> ReadAllAsync(CancellationToken ct);
    }
}
