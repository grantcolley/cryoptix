namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the credentials.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        public string? AccountName { get; set; }
        /// <summary>
        /// Gets or sets the api key.
        /// </summary>
        public string? ApiKey { get; set; }
        /// <summary>
        /// Gets or sets the api secret.
        /// </summary>
        public string? ApiSecret { get; set; }
        /// <summary>
        /// Gets or sets the api pass phrase.
        /// </summary>
        public string? ApiPassPhrase { get; set; }
    }
}