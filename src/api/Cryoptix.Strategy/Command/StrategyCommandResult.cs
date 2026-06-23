using Cryoptix.Strategy.State;

namespace Cryoptix.Strategy.Command
{
    /// <summary>
    /// Represents the strategy command result.
    /// </summary>
    public sealed class StrategyCommandResult
    {
        /// <summary>
        /// Gets or sets the success.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// Gets or sets the strategy status.
        /// </summary>
        public StrategyStatus? StrategyStatus { get; set; }
    }
}