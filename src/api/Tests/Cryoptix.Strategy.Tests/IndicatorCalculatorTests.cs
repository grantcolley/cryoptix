using Cryoptix.Market.Data;
using Cryoptix.Strategy.Calculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    public void CalculateEma_UsesPreviousEma()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        List<Kline> klines = RealisticKlines(start);

        decimal previousEma = 108m;
        decimal? ema = IndicatorCalculator.Ema(klines, 3, previousEma);

        Assert.IsTrue(ema.HasValue);
        Assert.AreEqual(109m, ema.Value);
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
