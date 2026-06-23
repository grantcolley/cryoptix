namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Defines the exchange api factory contract.
    /// </summary>
    public interface IExchangeApiFactory
    {
        /// <summary>
        /// Gets the API.
        /// </summary>
        /// <param name="exchange">The exchange.</param>
        /// <returns>The get API result.</returns>
        ExchangeApi GetApi(Market.Data.Exchange exchange);
        /// <summary>
        /// Gets the rest API.
        /// </summary>
        /// <param name="exchange">The exchange.</param>
        /// <returns>The get rest API result.</returns>
        IExchangeRestApi GetRestApi(Market.Data.Exchange exchange);
        /// <summary>
        /// Gets the subscription API.
        /// </summary>
        /// <param name="exchange">The exchange.</param>
        /// <returns>The get subscription API result.</returns>
        IExchangeSubscriptionApi GetSubscriptionApi(Market.Data.Exchange exchange);
    }
}
