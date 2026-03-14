using Cryoptix.Core.Exchanges;

namespace Cryoptix.Core.Api
{
    public interface IExchangeApiFactory
    {
        ExchangeApi GetApi(Exchange exchange);
        IExchangeRestApi GetRestApi(Exchange exchange);
        IExchangeSubscriptionApi GetSubscriptionApi(Exchange exchange);
    }
}
