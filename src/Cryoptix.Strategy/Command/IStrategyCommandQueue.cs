namespace Cryoptix.Strategy.Command
{
    public interface IStrategyCommandQueue
    {
        ValueTask EnqueueAsync(StrategyCommand command, CancellationToken ct);
        IAsyncEnumerable<StrategyCommand> ReadAllAsync(CancellationToken ct);
    }
}
