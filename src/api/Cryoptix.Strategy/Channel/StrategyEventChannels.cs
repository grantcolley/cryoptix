using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Channel
{
    /// <summary>
    /// Represents the strategy event channels.
    /// </summary>
    public sealed class StrategyEventChannels
    {
        /// <summary>
        /// Gets or sets the klines.
        /// </summary>
        public required Channel<KlineMarketEvent> Klines { get; init; }
        /// <summary>
        /// Gets or sets the trades.
        /// </summary>
        public required Channel<TradeMarketEvent> Trades { get; init; }
        /// <summary>
        /// Gets or sets the kline broadcasts.
        /// </summary>
        public required Channel<Kline> KlineBroadcasts { get; init; }
        /// <summary>
        /// Gets or sets the trade broadcasts.
        /// </summary>
        public required Channel<Trade> TradeBroadcasts { get; init; }
        /// <summary>
        /// Gets or sets the indicators broadcasts.
        /// </summary>
        public required Channel<Indicators> IndicatorsBroadcasts { get; init; }
        /// <summary>
        /// Gets or sets the signal broadcasts.
        /// </summary>
        public required Channel<Market.Strategy.Signal> SignalBroadcasts { get; init; }

        /// <summary>
        /// Executes the complete writers operation.
        /// </summary>
        /// <param name="error">The error value.</param>
        public void CompleteWriters(Exception? error = null)
        {
            Klines.Writer.TryComplete(error);
            Trades.Writer.TryComplete(error);
            KlineBroadcasts.Writer.TryComplete(error);
            TradeBroadcasts.Writer.TryComplete(error);
            IndicatorsBroadcasts.Writer.TryComplete(error);
            SignalBroadcasts.Writer.TryComplete(error);
        }
    }
}
