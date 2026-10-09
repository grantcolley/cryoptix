namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Defines the supported indicator parameter types.
    /// </summary>
    public enum IndicatorValueType
    {
        /// <summary>Specifies the calculation period.</summary>
        Period = 0,

        /// <summary>Specifies the slow moving average period.</summary>
        SlowPeriod = 1,

        /// <summary>Specifies the fast moving average period.</summary>
        FastPeriod = 2,

        /// <summary>Specifies the signal smoothing period.</summary>
        SignalPeriod = 3,
    }
}
