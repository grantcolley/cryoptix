namespace Cryoptix.Strategy.Order
{
    public interface IOrderExecutionService
    {
        Task<OrderExecutionResult> ExecuteAsync(
            OrderExecutionRequest request,
            CancellationToken cancellationToken);
    }
}
