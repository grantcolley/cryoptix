using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Command;
using Cryoptix.Web.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Cryoptix.Web.API.Tests;

[TestClass]
public sealed class StrategyBackgroundServiceTests
{
    [TestMethod]
    public async Task ProcessesStartUpdateAndStopCommands()
    {
        // Arrange
        var strategy = new Strategy.Strategies.Strategy { Name = "Demo" };
        var commands = new[]
        {
            new StrategyCommand { StrategyCommandType = StrategyCommandType.Start, Strategy = strategy },
            new StrategyCommand { StrategyCommandType = StrategyCommandType.Update, Strategy = strategy },
            new StrategyCommand { StrategyCommandType = StrategyCommandType.Stop }
        };
        var queue = new Mock<IStrategyCommandQueue>();
        var agent = new Mock<IStrategyAgent>(MockBehavior.Strict);
        queue.Setup(q => q.ReadAllAsync(It.IsAny<CancellationToken>())).Returns(Read(commands));
        agent.Setup(a => a.StartAsync(strategy, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        agent.Setup(a => a.UpdateAsync(strategy)).Returns(Task.CompletedTask);
        agent.Setup(a => a.StopAsync()).Returns(Task.CompletedTask);
        agent.Setup(a => a.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var service = new TestableStrategyBackgroundService(queue.Object, agent.Object);

        // Act
        await service.ExecuteForTestAsync(CancellationToken.None);

        // Verify
        agent.Verify(a => a.StartAsync(strategy, It.IsAny<CancellationToken>()), Times.Once);
        agent.Verify(a => a.UpdateAsync(strategy), Times.Once);
        agent.Verify(a => a.StopAsync(), Times.Once);
    }

    [TestMethod]
    public async Task IgnoresStartAndUpdateWithoutStrategyPayload()
    {
        // Arrage
        var commands = new[]
        {
            new StrategyCommand { StrategyCommandType = StrategyCommandType.Start },
            new StrategyCommand { StrategyCommandType = StrategyCommandType.Update },
            new StrategyCommand { StrategyCommandType = StrategyCommandType.Unknown }
        };
        var queue = new Mock<IStrategyCommandQueue>();
        var agent = new Mock<IStrategyAgent>(MockBehavior.Strict);
        queue.Setup(q => q.ReadAllAsync(It.IsAny<CancellationToken>())).Returns(Read(commands));
        agent.Setup(a => a.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var service = new TestableStrategyBackgroundService(queue.Object, agent.Object);

        // Act
        await service.ExecuteForTestAsync(CancellationToken.None);

        // Verify
        agent.Verify(a => a.StartAsync(It.IsAny<Strategy.Strategies.Strategy>(), It.IsAny<CancellationToken>()), Times.Never);
        agent.Verify(a => a.UpdateAsync(It.IsAny<Strategy.Strategies.Strategy>()), Times.Never);
        agent.Verify(a => a.StopAsync(), Times.Never);
    }

    [TestMethod]
    public async Task StrategyBackgroundService_StopAsync_StopsAgentAfterBaseStop()
    {
        // Arrange
        var queue = new Mock<IStrategyCommandQueue>();
        var agent = new Mock<IStrategyAgent>();
        queue.Setup(q => q.ReadAllAsync(It.IsAny<CancellationToken>())).Returns(Read([]));
        agent.Setup(a => a.StopAsync()).Returns(Task.CompletedTask);
        var service = new TestableStrategyBackgroundService(queue.Object, agent.Object);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Verify
        agent.Verify(a => a.StopAsync(), Times.Once);
    }

    private static async IAsyncEnumerable<StrategyCommand> Read(IEnumerable<StrategyCommand> commands)
    {
        foreach (StrategyCommand command in commands)
        {
            await Task.Yield();
            yield return command;
        }
    }

    private sealed class TestableStrategyBackgroundService(
        IStrategyCommandQueue queue,
        IStrategyAgent agent)
        : StrategyBackgroundService(queue, agent, NullLogger<StrategyBackgroundService>.Instance)
    {
        public Task ExecuteForTestAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);
    }
}
