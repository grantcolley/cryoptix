namespace Cryoptix.Exchange.Api
{
    public interface IExchangeApiFactory
    {
        ExchangeApi GetApi(Market.Models.Exchange exchange);
        IExchangeRestApi GetRestApi(Market.Models.Exchange exchange);
        IExchangeSubscriptionApi GetSubscriptionApi(Market.Models.Exchange exchange);
    }
}
