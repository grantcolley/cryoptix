using Cryoptix.Core.Exchanges;

namespace Cryoptix.Core.Api
{
    public sealed class ExchangeApiFactory(
        IEnumerable<IExchangeRestApi> restApis,
        IEnumerable<IExchangeSubscriptionApi> subscriptionApis) : IExchangeApiFactory
    {
        private readonly Dictionary<Exchange, IExchangeRestApi> _restApis = restApis.ToDictionary(x => x.Exchange);
        private readonly Dictionary<Exchange, IExchangeSubscriptionApi> _subscriptionApis = subscriptionApis.ToDictionary(x => x.Exchange);

        public ExchangeApi GetApi(Exchange exchange)
        {
            return new ExchangeApi
            {
                RestApi = GetRestApi(exchange),
                SubscriptionsApi = GetSubscriptionApi(exchange)
            };
        }

        public IExchangeRestApi GetRestApi(Exchange exchange)
        {
            if (_restApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No REST API registered for exchange '{exchange}'.");
        }

        public IExchangeSubscriptionApi GetSubscriptionApi(Exchange exchange)
        {
            if (_subscriptionApis.TryGetValue(exchange, out var api))
                return api;

            throw new NotSupportedException($"No subscription API registered for exchange '{exchange}'.");
        }
    }
}
