namespace Cryoptix.Exchange.Exceptions
{
    /// <summary>
    /// Represents the exchange api exception.
    /// </summary>
    [Serializable]
    public class ExchangeApiException(string message, string exchange, Exception? inner = null) : Exception(message, inner)
    {
        /// <summary>
        /// Gets the exchange.
        /// </summary>
        public string Exchange { get; } = exchange;
    }
}
