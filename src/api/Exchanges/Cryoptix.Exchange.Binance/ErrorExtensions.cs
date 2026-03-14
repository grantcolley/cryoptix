using CryptoExchange.Net.Objects;

namespace Cryoptix.Exchange.Binance
{
    public static class ErrorExtensions
    {
        public static string ToExchangeErrorMessage(this Error error)
        {
            return $"{error?.ErrorType} {error?.Code} {error?.Message} {error?.ErrorDescription}";
        }
    }
}
