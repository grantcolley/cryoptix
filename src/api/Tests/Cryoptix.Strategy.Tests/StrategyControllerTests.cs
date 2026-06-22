using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Controller;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.State;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class StrategyControllerTests
{
    [TestMethod]
    public async Task StartEnqueuesWhenProcessorExists()
    {
        // Arrange
        var queue = new Mock<IStrategyCommandQueue>(MockBehavior.Strict);
        var catalog = new Mock<IStrategyProcessorCatalog>(MockBehavior.Strict);
        var stateStore = new StrategyStateStore(NullLogger<StrategyStateStore>.Instance);
        var strategy = new Strategy.Strategies.Strategy { Name = "Demo", StrategyProcessorType = StrategyProcessorType.TradingFlow };
        Func<IStrategyProcessor> factory = () => Mock.Of<IStrategyProcessor>();

        catalog.SetupGet(c => c.Keys).Returns([StrategyProcessorType.TradingFlow]);
        catalog.Setup(c => c.TryCreate(StrategyProcessorType.TradingFlow, out factory)).Returns(true);
        queue.Setup(q => q.EnqueueAsync(It.Is<StrategyCommand>(c => c.StrategyCommandType == StrategyCommandType.Start && c.Strategy == strategy), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        var controller = new StrategyController(stateStore, queue.Object, catalog.Object);

        // Act
        StrategyCommandResult result = await controller.StartAsync(strategy, CancellationToken.None);

        // Assert / Verify
        Assert.IsTrue(result.Success);
        Assert.AreEqual(StrategyControllerStatusCodes.Status202Accepted, result.StatusCode);
        CollectionAssert.AreEqual(new[] { StrategyProcessorType.TradingFlow }, controller.GetAvailableStrategies().ToArray());
        queue.VerifyAll();
    }

    [TestMethod]
    public async Task StartReturnsNotFoundWhenProcessorMissing()
    {
        // Arrange
        var queue = new Mock<IStrategyCommandQueue>(MockBehavior.Strict);
        var catalog = new Mock<IStrategyProcessorCatalog>(MockBehavior.Strict);
        var stateStore = new StrategyStateStore(NullLogger<StrategyStateStore>.Instance);
        var strategy = new Strategy.Strategies.Strategy { Name = "Demo", StrategyProcessorType = StrategyProcessorType.TradingFlow };
        Func<IStrategyProcessor> factory = null!;
        catalog.Setup(c => c.TryCreate(StrategyProcessorType.TradingFlow, out factory)).Returns(false);
        var controller = new StrategyController(stateStore, queue.Object, catalog.Object);

        // Act
        StrategyCommandResult result = await controller.StartAsync(strategy, CancellationToken.None);

        // Assert / Verify
        Assert.IsFalse(result.Success);
        Assert.AreEqual(StrategyControllerStatusCodes.Status404NotFound, result.StatusCode);
        queue.Verify(q => q.EnqueueAsync(It.IsAny<StrategyCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAndStopEnqueueCommands()
    {
        // Arrange
        var queue = new Mock<IStrategyCommandQueue>(MockBehavior.Strict);
        var catalog = new Mock<IStrategyProcessorCatalog>(MockBehavior.Loose);
        var controller = new StrategyController(new StrategyStateStore(NullLogger<StrategyStateStore>.Instance), queue.Object, catalog.Object);
        var strategy = new Strategy.Strategies.Strategy { Name = "Demo" };

        queue.Setup(q => q.EnqueueAsync(It.Is<StrategyCommand>(c => c.StrategyCommandType == StrategyCommandType.Update && c.Strategy == strategy), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        queue.Setup(q => q.EnqueueAsync(It.Is<StrategyCommand>(c => c.StrategyCommandType == StrategyCommandType.Stop && c.Strategy == null), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        StrategyCommandResult update = await controller.UpdateAsync(strategy, CancellationToken.None);
        StrategyCommandResult stop = await controller.StopAsync(CancellationToken.None);

        // Assert / Verify
        Assert.IsTrue(update.Success);
        Assert.IsTrue(stop.Success);
        queue.VerifyAll();
    }
}
