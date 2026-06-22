using Cryoptix.Market.Data;
using Cryoptix.Strategy.Snapshot;

namespace Cryoptix.Strategy.Tests;

[TestClass]
public sealed class RealtimeStateTests
{
    [TestMethod]
    public void AccountRealtimeState_ClonesAndLooksUpBalancesCaseInsensitively()
    {
        AccountRealtimeState state = new();
        Account account = new() { Name = "acct" };
        account.Balances.Add(new Balance { Asset = "usdt", Free = 100m, Locked = 5m });

        state.Update(account);
        account.Balances[0].Free = 0m;

        Assert.IsTrue(state.TryGet(out Account? clone));
        Assert.IsNotNull(clone);
        Assert.AreEqual(100m, state.GetFreeBalance("USDT"));
        Assert.AreEqual(5m, state.GetLockedBalance("usdt"));
        Assert.AreEqual(105m, state.GetTotalBalance("UsDt"));
        Assert.IsTrue(state.HasFreeBalance("USDT", 99m));
        Assert.AreEqual(0m, state.GetFreeBalance("BTC"));
        Assert.ThrowsExactly<NotImplementedException>(() => state.GetPortfolioValue());
    }

    [TestMethod]
    public void OrderBookRealtimeState_ClonesOrderBook()
    {
        OrderBookRealtimeState state = new();
        OrderBook book = new()
        {
            Symbol = "BTCUSDT",
            LastUpdateId = 7,
            BestAsk = new OrderBookPrice { Price = 101m, Quantity = 1m },
            BestBid = new OrderBookPrice { Price = 100m, Quantity = 2m },
            Asks = [new OrderBookPrice { Price = 102m, Quantity = 3m }],
            Bids = [new OrderBookPrice { Price = 99m, Quantity = 4m }]
        };

        state.Update(book);
        book.BestAsk.Price = 1m;

        Assert.IsTrue(state.TryGet(out OrderBook? clone));
        Assert.IsNotNull(clone);
        Assert.AreEqual(101m, clone.BestAsk!.Price);
        clone.BestAsk.Price = 2m;
        Assert.IsTrue(state.TryGet(out OrderBook? secondClone));
        Assert.AreEqual(101m, secondClone!.BestAsk!.Price);
    }
}
