namespace Cryoptix.Core.Exceptions
{
    [Serializable]
    public class ExchangeApiException(string message, string exchange, Exception? inner = null) : Exception(message, inner)
    {
        public string Exchange { get; } = exchange;
    }
}
