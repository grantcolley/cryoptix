namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Defines the order status values.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Specifies the pending new value.
        /// </summary>
        PendingNew,
        /// <summary>
        /// Specifies the new value.
        /// </summary>
        New,
        /// <summary>
        /// Specifies the partially filled value.
        /// </summary>
        PartiallyFilled,
        /// <summary>
        /// Specifies the filled value.
        /// </summary>
        Filled,
        /// <summary>
        /// Specifies the canceled value.
        /// </summary>
        Canceled,
        /// <summary>
        /// Specifies the pending cancel value.
        /// </summary>
        PendingCancel,
        /// <summary>
        /// Specifies the rejected value.
        /// </summary>
        Rejected,
        /// <summary>
        /// Specifies the expired value.
        /// </summary>
        Expired,
        /// <summary>
        /// Specifies the unknown value.
        /// </summary>
        Unknown
    }
}