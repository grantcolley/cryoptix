namespace Cryoptix.Strategy.Clock
{
    public sealed class SystemStrategyClock : IStrategyClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
