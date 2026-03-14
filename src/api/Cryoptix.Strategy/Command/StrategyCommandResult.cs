using Cryoptix.Strategy.Status;

namespace Cryoptix.Strategy.Command
{
    public class StrategyCommandResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public StrategyStatus? StrategyStatus { get; set; }
    }
}