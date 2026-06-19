using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Processor;
using Microsoft.Extensions.Logging;

namespace Cryoptix.Strategy.Logging
{
    internal static partial class LogInformation
    {
        [LoggerMessage(
            EventId = 1000,
            Level = LogLevel.Information,
            Message = "KLINE {Source} {Symbol} {Interval} OpenTime:{OpenTime:u} CloseTime:{CloseTime:u} Open:{Open} Close:{Close}")]
        public static partial void KlineProcessed(
            ILogger logger,
            MarketEventSource source,
            string symbol,
            KlineInterval interval,
            DateTime openTime,
            DateTime closeTime,
            decimal open,
            decimal close);

        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Information,
            Message = "TRADE {Symbol} TradeId:{TradeId} Time:{Time:u} Price:{Price} BaseQuantity:{BaseQuantity} QuoteQuantity:{QuoteQuantity}")]
        public static partial void TradeProcessed(
            ILogger logger,
            string symbol,
                long TradeId,
                DateTime time,
                decimal price,
                decimal baseQuantity,
                decimal quoteQuantity);

        [LoggerMessage(
            EventId = 1002,
            Level = LogLevel.Information,
            Message = "Ignored duplicate trade {TradeId} for {Symbol}")]
        public static partial void TradeIgnored(
            ILogger logger,
            string symbol,
            long tradeId);

        [LoggerMessage(
            EventId = 1003,
            Level = LogLevel.Information,
            Message = "Execution requested for {Symbol}. Side:{Side} Quantity:{Quantity} LimitPrice:{LimitPrice} QuoteNotional:{QuoteNotional}")]
        public static partial void ExecutionRequested(
            ILogger logger,
            string symbol,
            OrderSide side,
            decimal quantity,
            decimal? limitPrice,
            decimal? quoteNotional);

        [LoggerMessage(
            EventId = 1004,
            Level = LogLevel.Information,
            Message = "Applied strategy update for {Symbol}. Subscriptions unchanged.")]
        public static partial void ApplyStrategyUpdate(
            ILogger logger,
            string symbol);

        [LoggerMessage(
            EventId = 1005,
            Level = LogLevel.Information,
            Message = "Fetching historical klines for {Symbol} {Interval} from {Start:u} to {End:u}")]
        public static partial void FetchHistoricalKlines(
            ILogger logger,
            string symbol,
            KlineInterval interval,
            DateTime start,
            DateTime end);

        [LoggerMessage(
            EventId = 1006,
            Level = LogLevel.Information,
            Message = "Seeded {Count} klines for {Symbol} {Interval}")]
        public static partial void SeededKlines(
            ILogger logger,
            int count,
            string symbol,
            KlineInterval interval);

        [LoggerMessage(
            EventId = 1007,
            Level = LogLevel.Information,
            Message = "Signal approved for execution for {Symbol} [{StrategyProcessorType}]: [{signalTimestampUtc}, {SignalType}, {signalReason}]. Side:{Side} Quantity:{Quantity} LimitPrice:{LimitPrice} QuoteNotional:{QuoteNotional} Reason:{Reason}")]
        public static partial void SignalApproved(
            ILogger logger,
            string symbol,
            StrategyProcessorType StrategyProcessorType,
            DateTime signalTimestampUtc,
            SignalType SignalType,
            string? signalReason,
            OrderSide side,
            decimal quantity,
            decimal? limitPrice,
            decimal? quoteNotional,
            string? reason);

        [LoggerMessage(
            EventId = 1008,
            Level = LogLevel.Information,
            Message = "Execution submitted for {Symbol}. Side:{Side} ExchangeOrderId:{ExchangeOrderId} Quantity:{Quantity} Price:{Price}")]
        public static partial void ExecutionSubmitted(
            ILogger logger,
            string symbol,
            OrderSide side,
            string? exchangeOrderId,
            decimal? quantity,
            decimal? Price);
    }
}
