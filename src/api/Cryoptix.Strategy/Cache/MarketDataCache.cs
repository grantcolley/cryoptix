using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Cache
{
    public sealed class MarketDataCache
    {
        private readonly int _maxTradesPerSymbol;
        private readonly int _maxKlinesPerSeries;

        private readonly Dictionary<(string Symbol, KlineInterval Interval), SortedDictionary<DateTime, Kline>> _klines = [];
        private readonly Dictionary<string, LinkedList<Trade>> _trades = [];
        private readonly Dictionary<string, HashSet<long>> _tradeIds = [];

        public MarketDataCache(int maxTradesPerSymbol, int maxKlinesPerSeries)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTradesPerSymbol);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxKlinesPerSeries);

            _maxTradesPerSymbol = maxTradesPerSymbol;
            _maxKlinesPerSeries = maxKlinesPerSeries;
        }

        public KlineUpsertResult UpsertKline(Kline kline)
        {
            ArgumentNullException.ThrowIfNull(kline);

            string symbol = NormalizeSymbol(kline.Symbol!);
            var key = (symbol, kline.Interval);

            if (!_klines.TryGetValue(key, out var series))
            {
                series = [];
                _klines[key] = series;
            }

            bool existed = series.TryGetValue(kline.OpenTime, out Kline? existing);
            series[kline.OpenTime] = kline;

            while (series.Count > _maxKlinesPerSeries)
            {
                DateTime oldestKey = series.First().Key;
                series.Remove(oldestKey);
            }

            if (!existed)
            {
                return new KlineUpsertResult(
                    inserted: true,
                    updated: false,
                    previous: null,
                    current: kline);
            }

            bool updated = !AreEquivalent(existing!, kline);

            return new KlineUpsertResult(
                inserted: false,
                updated: updated,
                previous: existing,
                current: kline);
        }

        public bool AddTrade(Trade trade)
        {
            ArgumentNullException.ThrowIfNull(trade);

            string symbol = NormalizeSymbol(trade.Symbol!);

            if (!_trades.TryGetValue(symbol, out var trades))
            {
                trades = [];
                _trades[symbol] = trades;
            }

            if (!_tradeIds.TryGetValue(symbol, out var tradeIds))
            {
                tradeIds = [];
                _tradeIds[symbol] = tradeIds;
            }

            if (!tradeIds.Add(trade.Id))
                return false;

            trades.AddLast(trade);

            while (trades.Count > _maxTradesPerSymbol)
            {
                LinkedListNode<Trade>? oldest = trades.First;
                if (oldest == null)
                    break;

                trades.RemoveFirst();
                tradeIds.Remove(oldest.Value.Id);
            }

            return true;
        }

        public IReadOnlyList<Kline> GetKlines(string symbol, KlineInterval interval)
        {
            var key = (NormalizeSymbol(symbol), interval);
            if (!_klines.TryGetValue(key, out var series))
                return [];

            return [.. series.Values];
        }

        public IReadOnlyList<Trade> GetTrades(string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (!_trades.TryGetValue(symbol, out var trades))
                return [];

            return [.. trades];
        }

        private static string NormalizeSymbol(string symbol) =>
            symbol.Trim().ToUpperInvariant();

        private static bool AreEquivalent(Kline x, Kline y)
        {
            return string.Equals(x.Symbol, y.Symbol, StringComparison.OrdinalIgnoreCase)
                && x.Interval == y.Interval
                && x.OpenTime == y.OpenTime
                && x.CloseTime == y.CloseTime
                && x.Open == y.Open
                && x.High == y.High
                && x.Low == y.Low
                && x.Close == y.Close
                && x.Volume == y.Volume
                && x.NumberOfTrades == y.NumberOfTrades
                && x.QuoteAssetVolume == y.QuoteAssetVolume
                && x.TakerBuyQuoteAssetVolume == y.TakerBuyQuoteAssetVolume
                && x.TakerBuyBaseAssetVolume == y.TakerBuyBaseAssetVolume
                && x.Final == y.Final;
        }
    }
}
