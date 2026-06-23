namespace Cryoptix.Strategy.Clock
{
    /// <summary>
    /// Represents the system strategy clock.
    /// </summary>
    public sealed class SystemStrategyClock : IStrategyClock
    {
        /// <summary>
        /// Gets or sets the utc now.
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
