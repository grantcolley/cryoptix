namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Defines the order execution service contract.
    /// </summary>
    public interface IOrderExecutionService
    {
        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<OrderExecutionResult> ExecuteAsync(
            /// <summary>
            /// Gets the value.
            /// </summary>
            OrderExecutionRequest request,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken);
    }
}
