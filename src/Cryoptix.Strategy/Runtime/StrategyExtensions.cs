namespace Cryoptix.Strategy.Runtime
{
    public static class StrategyExtensions
    {
        public static bool CanUpdate(this Strategy activeStrategy, Strategy? strategy, out string? message)
        {
            message = null;

            if(strategy == null)
            {
                message = "Strategy cannot be null.";
            }
            else if (activeStrategy.StrategyType != strategy.StrategyType)
            {
                message = $"StrategyType change from {activeStrategy.StrategyType} to {strategy.StrategyType} requires stop/start.";
            }
            else if (activeStrategy.StrategyId != strategy.StrategyId)
            {
                message = $"StrategyId change from StrategyId {activeStrategy.StrategyId} to {strategy.StrategyId} requires stop/start.";
            }
            else if (!string.Equals(activeStrategy.Symbol, strategy.Symbol, StringComparison.OrdinalIgnoreCase))
            {
                message = $"Strategy Name change from {activeStrategy.Name} to {strategy.Name} requires stop/start.";
            }
            else if (!string.Equals(activeStrategy.Symbol, strategy.Symbol, StringComparison.OrdinalIgnoreCase))
            {
                message = $"Strategy Symbol change from {activeStrategy.Symbol} to {strategy.Symbol} requires stop/start.";
            }

            return string.IsNullOrWhiteSpace(message);
        }
    }
}
