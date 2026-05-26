namespace Cryoptix.Strategy.Channel
{
    public interface IStrategyEventChannelFactory
    {
        StrategyEventChannels Create();
        StrategyEventChannels Create(Strategies.Strategy strategy);
        StrategyEventChannels Create(int klineCapacity, System.Threading.Channels.BoundedChannelFullMode klineFullMode, int tradeCapacity, System.Threading.Channels.BoundedChannelFullMode tradeFullMode, int klineBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode, int tradeBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode, int indicatorsBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode indicatorsBroadcastFullMode, int signalBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode signalBroadcastFullMode);
    }
}
