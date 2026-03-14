namespace Cryoptix.Exchange.Api
{
    public interface IExchangeApiFactory
    {
        ExchangeApi GetApi(Exchanges.Exchange exchange);
        IExchangeRestApi GetRestApi(Exchanges.Exchange exchange);
        IExchangeSubscriptionApi GetSubscriptionApi(Exchanges.Exchange exchange);
    }
}
