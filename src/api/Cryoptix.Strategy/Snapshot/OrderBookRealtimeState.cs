using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Snapshot
{
    /// <summary>
    /// Represents the order book realtime state.
    /// </summary>
    public sealed class OrderBookRealtimeState
    {
        private readonly Lock _gate = new();
        private OrderBook? _orderBook;

        /// <summary>
        /// Executes the update operation.
        /// </summary>
        /// <param name="orderBook">The order book value.</param>
        public void Update(OrderBook orderBook)
        {
            ArgumentNullException.ThrowIfNull(orderBook);

            lock (_gate)
            {
                _orderBook = Clone(orderBook);
            }
        }

        /// <summary>
        /// Executes the try get operation.
        /// </summary>
        /// <param name="orderBook">The order book value.</param>
        /// <returns>The try get result.</returns>
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
