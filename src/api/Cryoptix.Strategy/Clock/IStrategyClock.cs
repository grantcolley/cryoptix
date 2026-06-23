namespace Cryoptix.Strategy.Clock
{
    /// <summary>
    /// Defines the i strategy clock contract.
    /// </summary>
    public interface IStrategyClock
    {
        DateTime UtcNow { get; }
    }
}
