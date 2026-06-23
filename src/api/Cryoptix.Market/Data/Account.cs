namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the account.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Account"/> class.
        /// </summary>
        public Account()
        {
            Balances = [];
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the exchange.
        /// </summary>
        public Exchange Exchange { get; set; }
        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Gets or sets the buyer fee.
        /// </summary>
        public decimal BuyerFee { get; set; }
        /// <summary>
        /// Gets or sets the seller fee.
        /// </summary>
        public decimal SellerFee { get; set; }
        /// <summary>
        /// Gets the balances.
        /// </summary>
        public List<Balance> Balances { get; private set; }
    }
}
