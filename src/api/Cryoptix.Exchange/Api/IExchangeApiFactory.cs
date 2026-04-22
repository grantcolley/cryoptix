namespace Cryoptix.Exchange.Api
{
    public interface IExchangeApiFactory
    {
        ExchangeApi GetApi(Market.Data.Exchange exchange);
        IExchangeRestApi GetRestApi(Market.Data.Exchange exchange);
        IExchangeSubscriptionApi GetSubscriptionApi(Market.Data.Exchange exchange);
    }
}
