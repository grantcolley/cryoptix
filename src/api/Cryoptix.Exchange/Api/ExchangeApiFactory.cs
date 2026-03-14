namespace Cryoptix.Exchange.Api
{
    public sealed class ExchangeApiFactory(
        IEnumerable<IExchangeRestApi> restApis,
        IEnumerable<IExchangeSubscriptionApi> subscriptionApis) : IExchangeApiFactory
    {
        private readonly Dictionary<Exchanges.Exchange, IExchangeRestApi> _restApis = restApis.ToDictionary(x => x.Exchange);
        private readonly Dictionary<Exchanges.Exchange, IExchangeSubscriptionApi> _subscriptionApis = subscriptionApis.ToDictionary(x => x.Exchange);

        public ExchangeApi GetApi(Exchanges.Exchange exchange)
        {
            return new ExchangeApi
            {
                RestApi = GetRestApi(exchange),
                SubscriptionsApi = GetSubscriptionApi(exchange)
            };
        }

        public IExchangeRestApi GetRestApi(Exchanges.Exchange exchange)
        {
            if (_restApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No REST API registered for exchange '{exchange}'.");
        }

        public IExchangeSubscriptionApi GetSubscriptionApi(Exchanges.Exchange exchange)
        {
            if (_subscriptionApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No subscription API registered for exchange '{exchange}'.");
        }
    }
}
