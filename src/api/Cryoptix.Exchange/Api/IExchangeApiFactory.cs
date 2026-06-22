namespace Cryoptix.Exchange.Api
{
    /// <summary>
    /// Defines the i exchange api factory contract.
    /// </summary>
    public interface IExchangeApiFactory
    {
        ExchangeApi GetApi(Market.Data.Exchange exchange);
        IExchangeRestApi GetRestApi(Market.Data.Exchange exchange);
        IExchangeSubscriptionApi GetSubscriptionApi(Market.Data.Exchange exchange);
    }
}
