using Cryoptix.Market.Data;
using Cryoptix.Market.Strategy;
using Cryoptix.Strategy.Cache;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class MarketDataCacheTests
{
    [TestMethod]
    public void Constructor_RejectsNonPositiveLimits()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new MarketDataCache(0, 1, 1, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new MarketDataCache(1, 0, 1, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new MarketDataCache(1, 1, 0, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new MarketDataCache(1, 1, 1, 0));
    }

    [TestMethod]
    public void SetSymbols_NormalizesAndFindsExchangeSymbol()
    {
        // Arrange
        MarketDataCache cache = NewCache();

        cache.SetSymbols(
        [
            new Symbol { Name = "btc/usdt", ExchangeSymbol = "BTCUSDT", BaseAsset = "BTC", QuoteAsset = "USDT" },
            new Symbol { Name = " ", ExchangeSymbol = "IGNORED" }
        ]);

        // Act
        Symbol? result = cache.GetSymbolForStrategy(" btcusdt ");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("BTCUSDT", result.ExchangeSymbol);
        Assert.AreEqual("BTC", result.BaseAsset);
        Assert.IsNull(cache.GetSymbolForStrategy("ethusdt"));
        Assert.IsNull(cache.GetSymbolForStrategy(" "));
    }

    [TestMethod]
    public void UpsertKline_InsertsUpdatesAndTrimsOldest()
    {
        MarketDataCache cache = new(maxTradesPerSymbol: 5, maxKlinesPerSeries: 2, maxIndicatorsPerSeries: 5, maxSignalsPerSeries: 5);
        DateTime t0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Kline first = Kline(1, t0, 10m);
        Kline second = Kline(2, t0.AddMinutes(1), 11m);
        Kline third = Kline(3, t0.AddMinutes(2), 12m);

        var insert = cache.UpsertKline(first);
        var unchanged = cache.UpsertKline(Kline(1, t0, 10m));
        var changed = cache.UpsertKline(Kline(1, t0, 10.5m));
        cache.UpsertKline(second);
        cache.UpsertKline(third);

        IReadOnlyList<Kline> klines = cache.GetKlines("btcusdt", KlineInterval.Minute);

        Assert.IsTrue(insert.Inserted);
        Assert.IsFalse(insert.Updated);
        Assert.IsFalse(unchanged.Inserted);
        Assert.IsFalse(unchanged.Updated);
        Assert.IsFalse(changed.Inserted);
        Assert.IsTrue(changed.Updated);
        Assert.HasCount(2, klines);
        Assert.AreEqual(t0.AddMinutes(1), klines[0].OpenTime);
        Assert.AreEqual(t0.AddMinutes(2), klines[1].OpenTime);
    }

    private static readonly long[] expected = [2L, 3L];

    [TestMethod]
    public void AddTrade_DeduplicatesAndTrimsOldest()
    {
        MarketDataCache cache = new(maxTradesPerSymbol: 2, maxKlinesPerSeries: 5, maxIndicatorsPerSeries: 5, maxSignalsPerSeries: 5);

        Assert.IsTrue(cache.AddTrade(Trade(1, 100m)));
        Assert.IsFalse(cache.AddTrade(Trade(1, 100m)));
        Assert.IsTrue(cache.AddTrade(Trade(2, 101m)));
        Assert.IsTrue(cache.AddTrade(Trade(3, 102m)));

        IReadOnlyList<Trade> trades = cache.GetTrades("BTCUSDT");

        Assert.HasCount(2, trades);
        Assert.AreSequenceEqual(expected, [.. trades.Select(t => t.Id)]);
    }

    [TestMethod]
    public void IndicatorsAndSignals_AreUpsertedClonedAndTrimmed()
    {
        MarketDataCache cache = new(maxTradesPerSymbol: 5, maxKlinesPerSeries: 5, maxIndicatorsPerSeries: 1, maxSignalsPerSeries: 1);
        DateTime t0 = DateTime.UtcNow;

        cache.UpsertIndicators("btcusdt", new Market.Strategy.Indicators { TimestampUtc = t0, Series = new Dictionary<string, decimal> { ["fast"] = 1m }, Snapshots = new Dictionary<string, decimal> { ["fast"] = 10m } });
        cache.UpsertIndicators("btcusdt", new Market.Strategy.Indicators { TimestampUtc = t0.AddMinutes(1), Series = new Dictionary<string, decimal> { ["fast"] = 2m }, Snapshots = new Dictionary<string, decimal> { ["fast"] = 20m } });
        cache.UpsertSignal("btcusdt", new Market.Strategy.Signal { TimestampUtc = t0, SignalType = SignalType.Buy, Reason = "old" });
        cache.UpsertSignal("btcusdt", new Market.Strategy.Signal { TimestampUtc = t0.AddMinutes(1), SignalType = SignalType.Sell, Reason = "new" });

        IReadOnlyList<Market.Strategy.Indicators> indicators = cache.GetIndicators("BTCUSDT");
        IReadOnlyList<Market.Strategy.Signal> signals = cache.GetSignals("BTCUSDT");

        Assert.HasCount(1, indicators);
        Assert.AreEqual(2m, indicators[0].Series["fast"]);
        Assert.AreEqual(20m, indicators[0].Snapshots["fast"]);
        Assert.HasCount(1, signals);
        Assert.AreEqual(SignalType.Sell, signals[0].SignalType);
        Assert.AreEqual("new", signals[0].Reason);
    }

    [TestMethod]
    public void Rsis_AreUpsertedReplacedAndRetrieved()
    {
        MarketDataCache cache = NewCache();
        DateTime t0 = DateTime.UtcNow;

        var rsi1 = new Indicators.Rsi { Period = 3, PreviousClose = 100m, AverageGain = 1m, AverageLoss = 0m, Value = 100m, TimestampUtc = t0 };
        var rsi2 = new Indicators.Rsi { Period = 5, PreviousClose = 100m, AverageGain = 0.5m, AverageLoss = 0.5m, Value = 50m, TimestampUtc = t0 };

        // Upsert initial set
        cache.UpsertRsis("btcusdt", [ rsi1, rsi2 ]);

        IReadOnlyList<Indicators.Rsi> fetched = cache.GetRsis("BTCUSDT");
        Assert.HasCount(2, fetched);

        // Replace with a different set (only one period)
        var rsi3 = new Indicators.Rsi { Period = 3, PreviousClose = 101m, AverageGain = 2m, AverageLoss = 0m, Value = 100m, TimestampUtc = t0.AddMinutes(1) };
        cache.UpsertRsis("btcusdt", [ rsi3 ]);

        IReadOnlyList<Indicators.Rsi> fetched2 = cache.GetRsis("BTCUSDT");
        Assert.HasCount(1, fetched2);
        Assert.AreEqual(3, fetched2[0].Period);
        Assert.AreEqual(101m, fetched2[0].PreviousClose);
    }

    private static MarketDataCache NewCache() => new(5, 5, 5, 5);

    private static Kline Kline(long id, DateTime openTime, decimal close) => new()
    {
        Symbol = "BTCUSDT",
        Interval = KlineInterval.Minute,
        OpenTime = openTime,
        CloseTime = openTime.AddMinutes(1),
        Open = close - 1,
        High = close + 1,
        Low = close - 2,
        Close = close,
        Volume = id,
        NumberOfTrades = id,
        Final = true
    };

    private static Trade Trade(long id, decimal price) => new()
    {
        Symbol = "BTCUSDT",
        Id = id,
        Time = DateTime.UtcNow.AddSeconds(id),
        Price = price,
        BaseQuantity = 1m,
        QuoteQuantity = price
    };
}
