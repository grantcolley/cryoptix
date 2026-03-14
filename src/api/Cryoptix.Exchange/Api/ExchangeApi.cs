namespace Cryoptix.Exchange.Api
{
    public class ExchangeApi
    {
        public IExchangeRestApi? RestApi { get; init; }
        public IExchangeSubscriptionApi? SubscriptionsApi { get; init; }
    }
}
