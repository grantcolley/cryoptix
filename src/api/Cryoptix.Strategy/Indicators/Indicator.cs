namespace Cryoptix.Strategy.Indicators
{
    public class Indicator
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public IndicatorType IndicatorType { get; init; } = IndicatorType.Sma;
    }
}
