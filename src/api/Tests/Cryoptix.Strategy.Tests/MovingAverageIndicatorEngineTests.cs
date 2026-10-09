using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Engine.MovingAverage;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Snapshot;
using Cryoptix.Strategy.Indicators;
using Cryoptix.Strategy.Calculators;
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
        // Arrange
        MovingAverageIndicatorEngine engine = new(NullLogger<MovingAverageIndicatorEngine>.Instance);
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        StrategyAnalysisContext context = StrategyAnalysisContext(klines, [], klines[^1], new Strategies.Strategy
        {
            Symbol = "BTCUSDT",
            Indicators =
            [
                new Indicator { Name = "3 SMA", IndicatorType = IndicatorType.Sma, Values = [new IndicatorValue { Type = IndicatorValueType.Period, Value = 3 }] },
                new Indicator { Name = "5 SMA", IndicatorType = IndicatorType.Sma, Values = [new IndicatorValue { Type = IndicatorValueType.Period, Value = 5 }] },
                new Indicator { Name = "9 SMA", IndicatorType = IndicatorType.Sma, Values = [new IndicatorValue { Type = IndicatorValueType.Period, Value = 9 }] },
                new Indicator { Name = "too-long", IndicatorType = IndicatorType.Sma, Values = [new IndicatorValue { Type = IndicatorValueType.Period, Value = 20 }] },
                new Indicator { Name = "bad", IndicatorType = IndicatorType.Sma, Values = [new IndicatorValue { Type = IndicatorValueType.Period, Value = 0 }] }
            ]
        });

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        Assert.AreEqual(klines[^1].OpenTime, result.Indicators.TimestampUtc);

        Assert.AreEqual(109.66666666666666666666666667m, result.Indicators.Series["3 SMA"]);
        Assert.AreEqual(108.4m, result.Indicators.Series["5 SMA"]);
        Assert.AreEqual(105.77777777777777777777777778m, result.Indicators.Series["9 SMA"]);

        Assert.IsFalse(result.Indicators.Series.ContainsKey("too-long"));
        Assert.IsFalse(result.Indicators.Series.ContainsKey("bad"));
    }

    [TestMethod]
    public async Task ComputesEmaInitializedWithSmaWhenNoPreviousIndicator()
    {
        // Arrange
        MovingAverageIndicatorEngine engine = new(NullLogger<MovingAverageIndicatorEngine>.Instance);
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        StrategyAnalysisContext context = StrategyAnalysisContext(klines, [], klines[^1], new Strategies.Strategy
        {
            Symbol = "BTCUSDT",
            Indicators = [new Indicator { Name = "3 EMA", Values = [new IndicatorValue { Value = 3 }], IndicatorType = IndicatorType.Ema }]
        });

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        // EMA is seeded with the first-period SMA, then smoothed over remaining history.
        Assert.AreEqual(
            109.43229166666666666666666667m,
            result.Indicators.Series["3 EMA"],
            0.00000000000000000000000001m);
        Assert.HasCount(1, result.Emas);
        Assert.AreEqual(result.Indicators.Series["3 EMA"], result.Emas[0].Value);
        Assert.AreEqual(klines[^1].OpenTime, result.Indicators.TimestampUtc);
    }

    [TestMethod]
    public async Task ComputesEmaUsingPreviousEmaWhenPreviousIndicatorExists()
    {
        // Arrange
        MovingAverageIndicatorEngine engine = new(NullLogger<MovingAverageIndicatorEngine>.Instance);
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        Strategies.Strategy strategy = new()
        {
            Symbol = "BTCUSDT",
            Indicators = [new Indicator { Name = "3 EMA", IndicatorType = IndicatorType.Ema, Values = [new IndicatorValue { Type = IndicatorValueType.Period, Value = 3 }] }]
        };

        // Provide a previous EMA state before the current kline close time.
        Ema previousEma = new()
        {
            Period = 3,
            Value = 108m,
            TimestampUtc = klines[^1].CloseTime.AddMinutes(-1)
        };
        StrategyAnalysisContext context = StrategyAnalysisContext(
            klines,
            [],
            klines[^1],
            strategy,
            emas: [previousEma]);

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        // multiplier = 2/(3+1) = 0.5, latestClose = 110m => newEma = ((110 - 108) * 0.5) + 108 = 109m
        Assert.AreEqual(109m, result.Indicators.Series["3 EMA"]);
        Assert.HasCount(1, result.Emas);
        Assert.AreEqual(109m, result.Emas[0].Value);
    }

    [TestMethod]
    public async Task ReturnsEmptyForNonKlineEvent()
    {
        var engine = new MovingAverageIndicatorEngine(NullLogger<MovingAverageIndicatorEngine>.Instance);
        StrategyAnalysisContext context = StrategyAnalysisContext([], [], null, new Strategies.Strategy { Symbol = "BTCUSDT" }, MarketEventKind.Trade);

        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        Assert.AreEqual(DateTime.MinValue, result.Indicators.TimestampUtc);
        Assert.IsEmpty(result.Indicators.Series);
    }

    [TestMethod]
    public async Task ComputesRsiForConfiguredPeriod()
    {
        // Arrange
        MovingAverageIndicatorEngine engine = new(NullLogger<MovingAverageIndicatorEngine>.Instance);
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        StrategyAnalysisContext context = StrategyAnalysisContext(klines, [], klines[^1], new Strategies.Strategy
        {
            Symbol = "BTCUSDT",
            Indicators =
            [
                new Indicator { Name = "3 RSI", Values = [new IndicatorValue { Value = 3 }], IndicatorType = IndicatorType.Rsi }
            ]
        });

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.Rsis);
        Assert.HasCount(1, result.Rsis);
        Rsi? expected = IndicatorCalculator.RsiInitialize(klines, 3);
        Assert.IsNotNull(expected);
        Assert.AreEqual(expected!.Value, result.Rsis[0].Value);
        Assert.AreEqual(expected.Value, result.Indicators.Snapshots["3 RSI"]);
    }

    private static StrategyAnalysisContext StrategyAnalysisContext(
        IReadOnlyList<Kline> klines,
        IReadOnlyList<Market.Strategy.Indicators> indicators,
        Kline? currentKline,
        Strategies.Strategy strategy,
        MarketEventKind kind = MarketEventKind.Kline,
        IReadOnlyList<Ema>? emas = null)
    {
        return new StrategyAnalysisContext
        {
            ExchangeApi = new ExchangeApi(),
            Strategy = strategy,
            Klines = klines,
            Trades = [],
            Indicators = indicators,
            Emas = emas ?? [],
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
        Close = close,
        Final = true
    };
}
