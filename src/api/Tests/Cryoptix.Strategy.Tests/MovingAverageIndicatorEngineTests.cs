using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Engine.MovingAverage;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Snapshot;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class MovingAverageIndicatorEngineTests
{
    private static List<Kline> RealisticKlines(DateTime start) =>
    [
        Kline(start.AddMinutes(0), 100m),
        Kline(start.AddMinutes(1), 101m),
        Kline(start.AddMinutes(2), 103m),
        Kline(start.AddMinutes(3), 102m),
        Kline(start.AddMinutes(4), 104m),
        Kline(start.AddMinutes(5), 107m),
        Kline(start.AddMinutes(6), 106m),
        Kline(start.AddMinutes(7), 108m),
        Kline(start.AddMinutes(8), 111m),
        Kline(start.AddMinutes(9), 110m),
    ];

    [TestMethod]
    public async Task ComputesSmaForConfiguredPeriods()
    {
        var engine = new MovingAverageIndicatorEngine(NullLogger<MovingAverageIndicatorEngine>.Instance);
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        StrategyAnalysisContext context = StrategyAnalysisContext(klines, klines[^1], new Strategies.Strategy
        {
            Symbol = "BTCUSDT",
            Periods = new Dictionary<string, int>
            {
                ["3 SMA"] = 3,
                ["5 SMA"] = 5,
                ["9 SMA"] = 9,
                ["too-long"] = 20,
                ["bad"] = 0
            },
            SmoothingType = MovingAverageSmoothingType.Sma
        });

        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        Assert.AreEqual(klines[^1].CloseTime, result.Indicators.TimestampUtc);

        Assert.AreEqual(109.66666666666666666666666667m, result.Indicators.Values["3 SMA"]);
        Assert.AreEqual(108.4m, result.Indicators.Values["5 SMA"]);
        Assert.AreEqual(105.77777777777777777777777778m, result.Indicators.Values["9 SMA"]);

        Assert.IsFalse(result.Indicators.Values.ContainsKey("too-long"));
        Assert.IsFalse(result.Indicators.Values.ContainsKey("bad"));
    }

    [TestMethod]
    public async Task ReturnsEmptyForNonKlineEvent()
    {
        var engine = new MovingAverageIndicatorEngine(NullLogger<MovingAverageIndicatorEngine>.Instance);
        StrategyAnalysisContext context = StrategyAnalysisContext([], null, new Strategies.Strategy { Symbol = "BTCUSDT" }, MarketEventKind.Trade);

        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        Assert.AreEqual(DateTime.MinValue, result.Indicators.TimestampUtc);
        Assert.IsEmpty(result.Indicators.Values);
    }

    private static StrategyAnalysisContext StrategyAnalysisContext(
        IReadOnlyList<Kline> klines,
        Kline? currentKline,
        Strategies.Strategy strategy,
        MarketEventKind kind = MarketEventKind.Kline)
    {
        return new StrategyAnalysisContext
        {
            ExchangeApi = new ExchangeApi(),
            Strategy = strategy,
            Klines = klines,
            Trades = [],
            CurrentEvent = new MarketEventEnvelope
            {
                Kind = kind,
                Source = MarketEventSource.Live,
                Kline = currentKline,
                Trade = kind == MarketEventKind.Trade ? new Trade { Symbol = "BTCUSDT", Id = 1 } : null
            },
            AccountRealtimeState = new AccountRealtimeState(),
            OrderBookRealtimeState = new OrderBookRealtimeState()
        };
    }

    private static Kline Kline(DateTime openTime, decimal close) => new()
    {
        Symbol = "BTCUSDT",
        Interval = KlineInterval.Minute,
        OpenTime = openTime,
        CloseTime = openTime.AddMinutes(1),
        Close = close
    };
}
