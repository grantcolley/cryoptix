using Cryoptix.Exchange.Api;
using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Engine.MovingAverage;
using Cryoptix.Strategy.Event;
using Cryoptix.Strategy.Snapshot;
using Cryoptix.Strategy.Strategies;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;

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
            Periods = new Dictionary<string, Period>
            {
                ["3 SMA"] = new Period { Name = "3 SMA", Value = 3, SmoothingType = MovingAverageSmoothingType.Sma },
                ["5 SMA"] = new Period { Name = "5 SMA", Value = 5, SmoothingType = MovingAverageSmoothingType.Sma },
                ["9 SMA"] = new Period { Name = "9 SMA", Value = 9, SmoothingType = MovingAverageSmoothingType.Sma },
                ["too-long"] = new Period { Name = "too-long", Value = 20, SmoothingType = MovingAverageSmoothingType.Sma },
                ["bad"] = new Period { Name = "bad", Value = 0, SmoothingType = MovingAverageSmoothingType.Sma }
            }
        });

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        Assert.AreEqual(klines[^1].CloseTime, result.Indicators.TimestampUtc);

        Assert.AreEqual(109.66666666666666666666666667m, result.Indicators.Values["3 SMA"]);
        Assert.AreEqual(108.4m, result.Indicators.Values["5 SMA"]);
        Assert.AreEqual(105.77777777777777777777777778m, result.Indicators.Values["9 SMA"]);

        Assert.IsFalse(result.Indicators.Values.ContainsKey("too-long"));
        Assert.IsFalse(result.Indicators.Values.ContainsKey("bad"));
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
            Periods = new Dictionary<string, Period>
            {
                ["3 EMA"] = new Period { Name = "3 EMA", Value = 3, SmoothingType = MovingAverageSmoothingType.Ema }
            }
        });

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        // No previous EMA exists, so EMA should be initialized with SMA for the period
        Assert.AreEqual(109.66666666666666666666666667m, result.Indicators.Values["3 EMA"]);
        Assert.AreEqual(klines[^1].CloseTime, result.Indicators.TimestampUtc);
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
            Periods = new Dictionary<string, Period>
            {
                ["3 EMA"] = new Period { Name = "3 EMA", Value = 3, SmoothingType = MovingAverageSmoothingType.Ema }
            }
        };

        // Provide a previous EMA value that occurred before the current close time
        decimal previousEma = 108m;
        List<Indicators> previousIndicators =
        [
            new() {
                TimestampUtc = klines[^1].CloseTime.AddMinutes(-1),
                Values = new Dictionary<string, decimal> { ["3 EMA"] = previousEma }.ToImmutableDictionary()
            }
        ];

        StrategyAnalysisContext context = StrategyAnalysisContext(klines, previousIndicators, klines[^1], strategy);

        // Act
        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        // Assert
        // multiplier = 2/(3+1) = 0.5, latestClose = 110m => newEma = ((110 - 108) * 0.5) + 108 = 109m
        Assert.AreEqual(109m, result.Indicators.Values["3 EMA"]);
    }

    [TestMethod]
    public async Task ReturnsEmptyForNonKlineEvent()
    {
        var engine = new MovingAverageIndicatorEngine(NullLogger<MovingAverageIndicatorEngine>.Instance);
        StrategyAnalysisContext context = StrategyAnalysisContext([], [], null, new Strategies.Strategy { Symbol = "BTCUSDT" }, MarketEventKind.Trade);

        IndicatorComputationResult result = await engine.ComputeAsync(context, CancellationToken.None);

        Assert.AreEqual(DateTime.MinValue, result.Indicators.TimestampUtc);
        Assert.IsEmpty(result.Indicators.Values);
    }

    private static StrategyAnalysisContext StrategyAnalysisContext(
        IReadOnlyList<Kline> klines,
        IReadOnlyList<Indicators> indicators,
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
            Indicators = indicators,
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
