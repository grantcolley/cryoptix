namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Represents the exchange api factory.
    /// </summary>
    public sealed class ExchangeApiFactory(
        IEnumerable<IExchangeRestApi> restApis,
        IEnumerable<IExchangeSubscriptionApi> subscriptionApis) : IExchangeApiFactory
    {
        private readonly Dictionary<Market.Data.Exchange, IExchangeRestApi> _restApis = restApis.ToDictionary(x => x.Exchange);
        private readonly Dictionary<Market.Data.Exchange, IExchangeSubscriptionApi> _subscriptionApis = subscriptionApis.ToDictionary(x => x.Exchange);

        /// <summary>
        /// Executes the get api operation.
        /// </summary>
        /// <param name="exchange">The exchange value.</param>
        /// <returns>The get api result.</returns>
        public ExchangeApi GetApi(Market.Data.Exchange exchange)
        {
            return new ExchangeApi
            {
                RestApi = GetRestApi(exchange),
                SubscriptionsApi = GetSubscriptionApi(exchange)
            };
        }

        /// <summary>
        /// Executes the get rest api operation.
        /// </summary>
        /// <param name="exchange">The exchange value.</param>
        /// <returns>The get rest api result.</returns>
        public IExchangeRestApi GetRestApi(Market.Data.Exchange exchange)
        {
            if (_restApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No REST API registered for exchange '{exchange}'.");
        }

        /// <summary>
        /// Executes the get subscription api operation.
        /// </summary>
        /// <param name="exchange">The exchange value.</param>
        /// <returns>The get subscription api result.</returns>
        public IExchangeSubscriptionApi GetSubscriptionApi(Market.Data.Exchange exchange)
        {
            if (_subscriptionApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No subscription API registered for exchange '{exchange}'.");
        }
    }
}
