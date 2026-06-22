namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Represents the exchange api.
    /// </summary>
    public class ExchangeApi
    {
        /// <summary>
        /// Gets or sets the rest api.
        /// </summary>
        public IExchangeRestApi? RestApi { get; init; }
        /// <summary>
        /// Gets or sets the subscriptions api.
        /// </summary>
        public IExchangeSubscriptionApi? SubscriptionsApi { get; init; }
    }
}
