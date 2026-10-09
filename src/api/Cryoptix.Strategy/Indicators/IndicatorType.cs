namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Defines the supported indicator types.
    /// </summary>
    public enum IndicatorType
    {
        /// <summary>Specifies no indicator and no parameters.</summary>
        None = 0,

        /// <summary>Specifies a simple moving average.</summary>
        Sma = 1,

        /// <summary>Specifies an exponential moving average.</summary>
        Ema = 2,

        /// <summary>Specifies the relative strength index.</summary>
        Rsi = 3,

        /// <summary>Specifies moving average convergence divergence.</summary>
        Macd = 4,
    }
}
