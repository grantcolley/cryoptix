using Cryoptix.Strategy.State;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class StrategyStateStoreTestsTests
{
    [TestMethod]
    public void GetAndSetRoundTripsStatus()
    {
        // Arrange
        StrategyStateStore store = new(NullLogger<StrategyStateStore>.Instance);
        StrategyStatus status = new() { StrategyState = StrategyState.Running, Message = "ok" };

        // Act
        store.Set(status);

        // Assert
        Assert.AreSame(status, store.Get());
    }
}
