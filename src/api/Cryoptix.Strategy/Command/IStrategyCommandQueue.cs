namespace Cryoptix.Strategy.Command
{
    /// <summary>
    /// Defines the i strategy command queue contract.
    /// </summary>
    public interface IStrategyCommandQueue
    {
        ValueTask EnqueueAsync(StrategyCommand command, CancellationToken ct);
        IAsyncEnumerable<StrategyCommand> ReadAllAsync(CancellationToken ct);
    }
}
