using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class MarketEventSessionAccessorTests
{
    [TestMethod]
    public void SetTryGetAndClear()
    {
        // Arrange
        MarketEventSessionAccessor accessor = new();
        StrategyProcessorSession session = new()
        {
            ExchangeApi = new ExchangeApi(),
            Strategy = new Strategy.Strategies.Strategy(),
            Cache = new MarketDataCache(1, 1, 1, 1),
            OrderBookRealtimeState = new OrderBookRealtimeState(),
            AccountRealtimeState = new AccountRealtimeState()
        };

        // Assert / Act / Assert
        Assert.IsFalse(accessor.TryGetCurrent(out _));
        accessor.SetCurrent(session);
        Assert.IsTrue(accessor.TryGetCurrent(out StrategyProcessorSession? current));
        Assert.AreSame(session, current);
        accessor.ClearCurrent();
        Assert.IsFalse(accessor.TryGetCurrent(out _));
    }
}
