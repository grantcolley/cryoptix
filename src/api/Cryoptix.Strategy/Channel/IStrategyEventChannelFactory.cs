namespace Cryoptix.Strategy.Channel
{
    public interface IStrategyEventChannelFactory
    {
        StrategyEventChannels Create();
        StrategyEventChannels Create(Strategies.Strategy strategy);
        StrategyEventChannels Create(bool dropTradesWhenFull, int klineCapacity, System.Threading.Channels.BoundedChannelFullMode klineFullMode, int tradeCapacity, int klineBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode klineBroadcastFullMode, int tradeBroadcastCapacity, System.Threading.Channels.BoundedChannelFullMode tradeBroadcastFullMode);
    }
}
