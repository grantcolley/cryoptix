using Cryoptix.Strategy.Engine.MovingAverage;

namespace Cryoptix.Strategy.Strategies
{
    public class Period
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public MovingAverageSmoothingType SmoothingType { get; init; } = MovingAverageSmoothingType.Sma;
    }
}
