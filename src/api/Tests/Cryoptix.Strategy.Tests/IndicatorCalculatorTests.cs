using Cryoptix.Market.Data;
using Cryoptix.Strategy.Calculators;
using Cryoptix.Strategy.Indicators;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class IndicatorCalculatorTests
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
    public void CalculateSma_ComputesExpected()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        decimal? sma = IndicatorCalculator.Sma(klines, 3);

        Assert.IsTrue(sma.HasValue);
        Assert.AreEqual(109.66666666666666666666666667m, sma.Value);
    }

    [TestMethod]
    public void EmaInitialize_SeedsAndSmoothsFinalizedHistory()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = [
            Kline(start, 100m),
            Kline(start.AddMinutes(1), 102m),
            Kline(start.AddMinutes(2), 104m)
        ];

        Ema? ema = IndicatorCalculator.EmaInitialize(klines, 2);

        Assert.IsNotNull(ema);
        Assert.AreEqual(103m, ema!.Value);
        Assert.AreEqual(klines[^1].CloseTime, ema.TimestampUtc);
    }

    [TestMethod]
    public void EmaInitialize_IgnoresTrailingNonFinalKline()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = [
            Kline(start, 100m),
            Kline(start.AddMinutes(1), 102m),
            Kline(start.AddMinutes(2), 104m),
            new() { Symbol = "BTCUSDT", Interval = KlineInterval.Minute, OpenTime = start.AddMinutes(3), CloseTime = start.AddMinutes(4), Close = 500m, Final = false }
        ];

        Ema? ema = IndicatorCalculator.EmaInitialize(klines, 2);

        Assert.IsNotNull(ema);
        Assert.AreEqual(103m, ema!.Value);
        Assert.AreEqual(klines[2].CloseTime, ema.TimestampUtc);
    }

    [TestMethod]
    public void EmaInitialize_InvalidPeriodOrInsufficientFinalizedKlines_ReturnsNull()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = [Kline(start, 100m), Kline(start.AddMinutes(1), 102m)];

        Assert.IsNull(IndicatorCalculator.EmaInitialize(klines, 0));
        Assert.IsNull(IndicatorCalculator.EmaInitialize(klines, 3));
    }

    [TestMethod]
    public void EmaUpdate_UsesPreviousStateAndNewKline()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> initialKlines = [Kline(start, 100m), Kline(start.AddMinutes(1), 102m)];
        Ema? initial = IndicatorCalculator.EmaInitialize(initialKlines, 2);
        Assert.IsNotNull(initial);

        Kline next = Kline(start.AddMinutes(2), 104m);
        Ema updated = IndicatorCalculator.EmaUpdate(initial!, next);

        Assert.AreEqual(103m, updated.Value);
        Assert.AreEqual(next.CloseTime, updated.TimestampUtc);
        Assert.AreEqual(101m, initial.Value);
    }

    [TestMethod]
    public void EmaUpdate_WithNonNewerKline_ReturnsPreviousState()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Ema? initial = IndicatorCalculator.EmaInitialize([Kline(start, 100m), Kline(start.AddMinutes(1), 102m)], 2);
        Assert.IsNotNull(initial);

        Ema updated = IndicatorCalculator.EmaUpdate(initial!, Kline(start.AddMinutes(1), 105m));

        Assert.AreSame(initial, updated);
        Assert.AreEqual(101m, updated.Value);
    }

    [TestMethod]
    public void RsiInitialize_ComputesExpected()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = [
            Kline(start.AddMinutes(0), 100m),
            Kline(start.AddMinutes(1), 101m),
            Kline(start.AddMinutes(2), 102m),
            Kline(start.AddMinutes(3), 103m)
        ];

        // All gains: RSI should be 100
        Rsi? rsi = IndicatorCalculator.RsiInitialize(klines, 3);

        Assert.IsNotNull(rsi);
        Assert.AreEqual(103m, rsi!.PreviousClose);
        Assert.AreEqual(1m, rsi.AverageGain);
        Assert.AreEqual(0m, rsi.AverageLoss);
        Assert.AreEqual(100m, rsi.Value);
    }

    [TestMethod]
    public void Rsi_PeriodLessOrEqualZero_ReturnsNull()
    {
        Assert.IsNull(IndicatorCalculator.RsiInitialize([], 0));
    }

    [TestMethod]
    public void Rsi_InsufficientFinalizedCandles_ReturnsNull()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> tooFew = [ Kline(start, 100m) ];
        Assert.IsNull(IndicatorCalculator.RsiInitialize(tooFew, 3));
    }

    [TestMethod]
    public void Rsi_ExactPeriodPlusOne_ReturnsInitialRsi()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> exact = [ Kline(start, 100m), Kline(start.AddMinutes(1), 101m), Kline(start.AddMinutes(2), 102m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? init = IndicatorCalculator.RsiInitialize(exact, 3);
        Assert.IsNotNull(init);
    }

    [TestMethod]
    public void Rsi_IgnoresTrailingUnfinishedCandle()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> exact = [ Kline(start, 100m), Kline(start.AddMinutes(1), 101m), Kline(start.AddMinutes(2), 102m), Kline(start.AddMinutes(3), 103m) ];
        var unfinal = exact.ToList();
        var unfinished = new Kline { Symbol = "BTCUSDT", Interval = KlineInterval.Minute, OpenTime = start.AddMinutes(4), CloseTime = start.AddMinutes(5), Close = 104m, Final = false };
        unfinal.Add(unfinished);
        Assert.IsNotNull(IndicatorCalculator.RsiInitialize(unfinal, 3));
    }

    [TestMethod]
    public void Rsi_AllIncreasing_Returns100()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> increasing = [ Kline(start, 100m), Kline(start.AddMinutes(1), 101m), Kline(start.AddMinutes(2), 102m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? r1 = IndicatorCalculator.RsiInitialize(increasing, 3);
        Assert.IsNotNull(r1);
        Assert.AreEqual(100m, r1!.Value);
    }

    [TestMethod]
    public void Rsi_AllDecreasing_Returns0()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> decreasing = [ Kline(start, 103m), Kline(start.AddMinutes(1), 102m), Kline(start.AddMinutes(2), 101m), Kline(start.AddMinutes(3), 100m) ];
        Rsi? r2 = IndicatorCalculator.RsiInitialize(decreasing, 3);
        Assert.IsNotNull(r2);
        Assert.AreEqual(0m, r2!.Value);
    }

    [TestMethod]
    public void Rsi_AllUnchanged_Returns50()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> unchanged = [ Kline(start, 100m), Kline(start.AddMinutes(1), 100m), Kline(start.AddMinutes(2), 100m), Kline(start.AddMinutes(3), 100m) ];
        Rsi? r3 = IndicatorCalculator.RsiInitialize(unchanged, 3);
        Assert.IsNotNull(r3);
        Assert.AreEqual(50m, r3!.Value);
    }

    [TestMethod]
    public void Rsi_MixedGainsLosses_ReturnsKnownValue()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> mixed = [ Kline(start, 100m), Kline(start.AddMinutes(1), 102m), Kline(start.AddMinutes(2), 101m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? r4 = IndicatorCalculator.RsiInitialize(mixed, 3);
        Assert.IsNotNull(r4);
        Assert.AreEqual(80m, r4!.Value);
    }

    [TestMethod]
    public void Rsi_UpdateWithNonFinalKline_ReturnsLiveRsi()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> mixed = [ Kline(start, 100m), Kline(start.AddMinutes(1), 102m), Kline(start.AddMinutes(2), 101m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? r4 = IndicatorCalculator.RsiInitialize(mixed, 3);
        Assert.IsNotNull(r4);

        Kline nextUnfinal = new() { Symbol = "BTCUSDT", Interval = KlineInterval.Minute, OpenTime = start.AddMinutes(4), CloseTime = start.AddMinutes(5), Close = 105m, Final = false };
        Rsi? stayed = IndicatorCalculator.RsiUpdate(r4!, nextUnfinal);
        Assert.IsNotNull(stayed);
        Assert.AreEqual(87.5m, stayed.Value);
    }

    [TestMethod]
    public void Rsi_UpdateWithOldKline_ReturnsPreviousRsi()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> mixed = [ Kline(start, 100m), Kline(start.AddMinutes(1), 102m), Kline(start.AddMinutes(2), 101m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? r4 = IndicatorCalculator.RsiInitialize(mixed, 3);
        Assert.IsNotNull(r4);

        Kline old = new() { Symbol = "BTCUSDT", Interval = KlineInterval.Minute, OpenTime = start.AddMinutes(3), CloseTime = start.AddMinutes(4), Close = 103m, Final = true };
        Rsi? stayed2 = IndicatorCalculator.RsiUpdate(r4!, old);
        Assert.IsNotNull(stayed2);
        Assert.AreEqual(r4.Value, stayed2.Value);
    }

    [TestMethod]
    public void Rsi_UpdateWithNextFinalKline_ReturnsNewRsi()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> mixed = [ Kline(start, 100m), Kline(start.AddMinutes(1), 102m), Kline(start.AddMinutes(2), 101m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? r4 = IndicatorCalculator.RsiInitialize(mixed, 3);
        Assert.IsNotNull(r4);

        Kline next = Kline(start.AddMinutes(4), 100m);
        Rsi? updated = IndicatorCalculator.RsiUpdate(r4!, next);
        Assert.IsNotNull(updated);
        Assert.AreNotEqual(r4.Value, updated.Value);
    }

    [TestMethod]
    public void Rsi_HistoricalInitializationMatchesIncremental()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> mixed = [ Kline(start, 100m), Kline(start.AddMinutes(1), 102m), Kline(start.AddMinutes(2), 101m), Kline(start.AddMinutes(3), 103m) ];
        Rsi? r4 = IndicatorCalculator.RsiInitialize(mixed, 3);
        Assert.IsNotNull(r4);

        Kline next = Kline(start.AddMinutes(4), 100m);
        var historyWithNext = mixed.ToList();
        historyWithNext.Add(next);

        Rsi? fromHistory = IndicatorCalculator.RsiInitialize(historyWithNext, 3);
        Assert.IsNotNull(fromHistory);

        Rsi? viaIncrement = IndicatorCalculator.RsiUpdate(r4!, next);
        Assert.IsNotNull(viaIncrement);
        Assert.AreEqual(fromHistory.Value, viaIncrement.Value);
    }

    [TestMethod]
    public void Rsi_UpdateUsesPreviousState()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // Initial history: 100,102,101,103 -> changes +2,-1,+2 -> avgGain=4/3, avgLoss=1/3 -> RSI=80
        List<Kline> history = [
            Kline(start.AddMinutes(0), 100m),
            Kline(start.AddMinutes(1), 102m),
            Kline(start.AddMinutes(2), 101m),
            Kline(start.AddMinutes(3), 103m)
        ];

        Rsi? initial = IndicatorCalculator.RsiInitialize(history, 3);
        Assert.IsNotNull(initial);
        Assert.AreEqual(80m, initial!.Value);

        // New kline with a loss of 3 (103 -> 100) should update RSI
        Kline next = Kline(start.AddMinutes(4), 100m);
        Rsi? updated = IndicatorCalculator.RsiUpdate(initial!, next);

        Assert.IsNotNull(updated);

        decimal expected = 800m / 19m; // calculated exact: 800/19 ~= 42.1052631578947368
        decimal diff = Math.Abs(updated!.Value - expected);
        Assert.IsLessThan(0.0000000000001m, diff, $"Expected approx {expected}, got {updated.Value}");
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
