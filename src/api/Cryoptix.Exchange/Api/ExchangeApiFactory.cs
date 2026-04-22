namespace Cryoptix.Exchange.Api
{
    public sealed class ExchangeApiFactory(
        IEnumerable<IExchangeRestApi> restApis,
        IEnumerable<IExchangeSubscriptionApi> subscriptionApis) : IExchangeApiFactory
    {
        private readonly Dictionary<Market.Data.Exchange, IExchangeRestApi> _restApis = restApis.ToDictionary(x => x.Exchange);
        private readonly Dictionary<Market.Data.Exchange, IExchangeSubscriptionApi> _subscriptionApis = subscriptionApis.ToDictionary(x => x.Exchange);

        public ExchangeApi GetApi(Market.Data.Exchange exchange)
        {
            return new ExchangeApi
            {
                RestApi = GetRestApi(exchange),
                SubscriptionsApi = GetSubscriptionApi(exchange)
            };
        }

        public IExchangeRestApi GetRestApi(Market.Data.Exchange exchange)
        {
            if (_restApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No REST API registered for exchange '{exchange}'.");
        }

        public IExchangeSubscriptionApi GetSubscriptionApi(Market.Data.Exchange exchange)
        {
            if (_subscriptionApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No subscription API registered for exchange '{exchange}'.");
        }
    }
}
