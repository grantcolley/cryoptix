using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Logging
{
    internal static partial class LogDebug
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "Dropped indicators broadcast for {Symbol} due to channel pressure.")]
        public static partial void IndicatorsDropped(
            ILogger logger,
            string symbol);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "Dropped signal broadcast for {Symbol} due to channel pressure.")]
        public static partial void SignalDropped(
            ILogger logger,
            string symbol);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Debug,
            Message = "Dropped kline broadcast event for {Symbol} {Interval} due to broadcast channel pressure.")]
        public static partial void KlineDropped(
            ILogger logger,
            string symbol,
            KlineInterval interval);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Debug,
            Message = "Dropped trade broadcast event for {Symbol} TradeId:{TradeId} due to broadcast channel pressure.")]
        public static partial void TradeDropped(
            ILogger logger,
            string symbol,
            long tradeId);

        [LoggerMessage(
            EventId = 5,
            Level = LogLevel.Debug,
            Message = "Computed indicators for {Symbol}. Values:{Values}")]
        public static partial void IndicatorsComputed(
            ILogger logger,
            string symbol,
            Dictionary<string, decimal> values);

        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Debug,
            Message = "Sizing skipped for {Symbol}: signal {TimeStamp} {SignalType} {Reason} is not executable.")]
        public static partial void SizingSkipped(
            ILogger logger,
            string symbol,
            DateTime timeStamp,
            SignalType signalType,
            string? reason);
    }
}
