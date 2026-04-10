namespace Cryoptix.Strategy.Clock
{
    public interface IStrategyClock
    {
        DateTime UtcNow { get; }
    }
}
