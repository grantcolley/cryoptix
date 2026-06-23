using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Represents the kline upsert result.
    /// </summary>
    public readonly struct KlineUpsertResult(
        bool inserted,
        bool updated,
        Kline? previous,
        Kline current)
    {
        /// <summary>
        /// Gets the inserted.
        /// </summary>
        public bool Inserted { get; } = inserted;
        /// <summary>
        /// Gets the updated.
        /// </summary>
        public bool Updated { get; } = updated;
        /// <summary>
        /// Gets the previous.
        /// </summary>
        public Kline? Previous { get; } = previous;
        /// <summary>
        /// Gets the current.
        /// </summary>
        public Kline Current { get; } = current ?? throw new ArgumentNullException(nameof(current));
    }
}
