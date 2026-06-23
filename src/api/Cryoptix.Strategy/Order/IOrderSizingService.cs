using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;

namespace Cryoptix.Strategy.Order
{
    /// <summary>
    /// Defines the order sizing service contract.
    /// </summary>
    public interface IOrderSizingService
    {
        /// <summary>
        /// Sizes the operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="signal">The signal.</param>
        /// <param name="orderBook">The order book.</param>
        /// <param name="account">The account.</param>
        /// <returns>The size result.</returns>
        OrderSizingResult? Size(
            /// <summary>
            /// Gets the value.
            /// </summary>
            StrategyAnalysisContext context,
            /// <summary>
            /// Gets the value.
            /// </summary>
            SignalEvaluationResult signal,
            /// <summary>
            /// Gets the value.
            /// </summary>
            OrderBook orderBook,
            /// <summary>
            /// Gets the value.
            /// </summary>
            Account account);
    }
}
