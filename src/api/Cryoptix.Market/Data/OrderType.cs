namespace Cryoptix.Market.Data
{
    /// <summary>
    /// Defines the order type values.
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Specifies the limit value.
        /// </summary>
        Limit = 0,
        /// <summary>
        /// Specifies the market value.
        /// </summary>
        Market = 1,
        /// <summary>
        /// Specifies the stop loss value.
        /// </summary>
        StopLoss = 2,
        /// <summary>
        /// Specifies the stop loss limit value.
        /// </summary>
        StopLossLimit = 3,
        /// <summary>
        /// Specifies the take profit value.
        /// </summary>
        TakeProfit = 4,
        /// <summary>
        /// Specifies the take profit limit value.
        /// </summary>
        TakeProfitLimit = 5,
        /// <summary>
        /// Specifies the limit maker value.
        /// </summary>
        LimitMaker = 6
    }
}
