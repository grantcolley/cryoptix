using Cryoptix.Exchange.Models;

namespace Cryoptix.Strategy.Snapshot
{
    public sealed class OrderBookRealtimeState
    {
        private readonly object _gate = new();
        private OrderBook? _orderBook;

        public void Update(OrderBook orderBook)
        {
            ArgumentNullException.ThrowIfNull(orderBook);

            lock (_gate)
            {
                _orderBook = Clone(orderBook);
            }
        }

        public bool TryGet(out OrderBook? orderBook)
        {
            lock (_gate)
            {
                orderBook = _orderBook == null ? null : Clone(_orderBook);
                return orderBook != null;
            }
        }

        private static OrderBook Clone(OrderBook source)
        {
            return new OrderBook
            {
                Symbol = source.Symbol,
                Exchange = source.Exchange,
                LastUpdateId = source.LastUpdateId,
                UpdateTime = source.UpdateTime,
                BestAsk = source.BestAsk == null
                    ? null
                    : new OrderBookPrice
                    {
                        Price = source.BestAsk.Price,
                        Quantity = source.BestAsk.Quantity
                    },
                BestBid = source.BestBid == null
                    ? null
                    : new OrderBookPrice
                    {
                        Price = source.BestBid.Price,
                        Quantity = source.BestBid.Quantity
                    },
                Asks = source.Asks?.Select(x => new OrderBookPrice
                {
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Bids = source.Bids?.Select(x => new OrderBookPrice
                {
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList()
            };
        }
    }
}
