namespace Cryoptix.Strategy.Execution
{
    public interface IOrderExecutionService
    {
        Task<OrderExecutionResult> ExecuteAsync(
            OrderExecutionRequest request,
            CancellationToken cancellationToken);
    }
}
