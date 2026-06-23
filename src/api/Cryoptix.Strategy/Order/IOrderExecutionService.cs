namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Defines the i order execution service contract.
    /// </summary>
    public interface IOrderExecutionService
    {
        Task<OrderExecutionResult> ExecuteAsync(
            OrderExecutionRequest request,
            CancellationToken cancellationToken);
    }
}
