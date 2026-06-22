namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Represents the balance.
    /// </summary>
    public class Balance
    {
        /// <summary>
        /// Gets or sets the asset.
        /// </summary>
        public string? Asset { get; set; }
        /// <summary>
        /// Gets or sets the free.
        /// </summary>
        public decimal Free { get; set; }
        /// <summary>
        /// Gets or sets the locked.
        /// </summary>
        public decimal Locked { get; set; }
        /// <summary>
        /// Gets the total.
        /// </summary>
        public decimal Total => Free + Locked;
    }
}