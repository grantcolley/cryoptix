using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Cache
{
    public readonly struct KlineUpsertResult(
        bool inserted,
        bool updated,
        Kline? previous,
        Kline current)
    {
        public bool Inserted { get; } = inserted;
        public bool Updated { get; } = updated;
        public Kline? Previous { get; } = previous;
        public Kline Current { get; } = current ?? throw new ArgumentNullException(nameof(current));
    }
}
