namespace Cryoptix.Strategy.Clock
{
    /// <summary>
    /// Defines the strategy clock contract.
    /// </summary>
    public interface IStrategyClock
    {
        /// <summary>
        /// Gets the UTC now.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
