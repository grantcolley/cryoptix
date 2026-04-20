using Cryoptix.Market.Models;

namespace Cryoptix.Strategy.Cache
{
    public readonly struct KlineUpsertResult
    {
        public KlineUpsertResult(
            bool inserted,
            bool updated,
            Kline? previous,
            Kline current)
        {
            Inserted = inserted;
            Updated = updated;
            Previous = previous;
            Current = current ?? throw new ArgumentNullException(nameof(current));
        }

        public bool Inserted { get; }
        public bool Updated { get; }
        public Kline? Previous { get; }
        public Kline Current { get; }
    }
}
