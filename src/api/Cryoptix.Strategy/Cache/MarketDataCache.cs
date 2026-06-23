using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Cache
{
    /// <summary>
    /// Represents the market data cache.
    /// </summary>
    public sealed class MarketDataCache
    {
        private readonly int _maxTradesPerSymbol;
        private readonly int _maxKlinesPerSeries;
        private readonly int _maxIndicatorsPerSeries;
        private readonly int _maxSignalsPerSeries;

        private readonly Lock _symbolsGate = new();
        private readonly Lock _klinesGate = new();
        private readonly Lock _tradesGate = new();
        private readonly Lock _indicatorsGate = new();
        private readonly Lock _signalsGate = new();

        private readonly Dictionary<(string Symbol, KlineInterval Interval), SortedDictionary<DateTime, Kline>> _klines = [];
        private readonly Dictionary<string, LinkedList<Trade>> _trades = [];
        private readonly Dictionary<string, HashSet<long>> _tradeIds = [];
        private readonly Dictionary<string, SortedDictionary<DateTime, Market.Strategy.Indicators>> _indicators = [];
        private readonly Dictionary<string, SortedDictionary<DateTime, Market.Strategy.Signal>> _signals = [];
        private readonly HashSet<Symbol> _symbols = new(SymbolComparer.Instance);

        /// <summary>
        /// Initializes a new instance of the &lt;see cref="MarketDataCache"/&gt; class.
        /// </summary>
        /// <param name="maxTradesPerSymbol">The max trades per symbol value.</param>
        /// <param name="maxKlinesPerSeries">The max klines per series value.</param>
        /// <param name="maxIndicatorsPerSeries">The max indicators per series value.</param>
        /// <param name="maxSignalsPerSeries">The max signals per series value.</param>
        public MarketDataCache(int maxTradesPerSymbol, int maxKlinesPerSeries, int maxIndicatorsPerSeries, int maxSignalsPerSeries)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTradesPerSymbol);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxKlinesPerSeries);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxIndicatorsPerSeries);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSignalsPerSeries);

            _maxTradesPerSymbol = maxTradesPerSymbol;
            _maxKlinesPerSeries = maxKlinesPerSeries;
            _maxIndicatorsPerSeries = maxIndicatorsPerSeries;
            _maxSignalsPerSeries = maxSignalsPerSeries;
        }

        /// <summary>
        /// Executes the set symbols operation.
        /// </summary>
        /// <param name="symbols">The symbols value.</param>
        public void SetSymbols(IEnumerable<Symbol> symbols)
        {
            ArgumentNullException.ThrowIfNull(symbols);

            lock (_symbolsGate)
            {
                _symbols.Clear();

                foreach (var s in symbols)
                {
                    if (s == null || string.IsNullOrWhiteSpace(s.Name))
                        continue;

                    _symbols.Add(new Symbol
                    {
                        Name = NormalizeSymbol(s.Name),
                        Exchange = s.Exchange,
                        NameDelimiter = s.NameDelimiter,
                        ExchangeSymbol = s.ExchangeSymbol,
                        BaseAsset = s.BaseAsset,
                        BaseAssetPrecision = s.BaseAssetPrecision,
                        QuoteAsset = s.QuoteAsset,
                        QuoteAssetPrecision = s.QuoteAssetPrecision,
                        NotionalMinimumValue = s.NotionalMinimumValue,
                        TickSize = s.TickSize,
                        LotSize = s.LotSize
                    });
                }
            }
        }

        /// <summary>
        /// Executes the get symbol for strategy operation.
        /// </summary>
        /// <param name="strategySymbol">The strategy symbol value.</param>
        /// <returns>The get symbol for strategy result.</returns>
        public Symbol? GetSymbolForStrategy(string strategySymbol)
        {
            if (string.IsNullOrWhiteSpace(strategySymbol))
                return null;

            string normalized = NormalizeSymbol(strategySymbol);

            lock (_symbolsGate)
            {
                return _symbols.FirstOrDefault(s =>
                    string.Equals(s.ExchangeSymbol, normalized, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Executes the upsert indicators operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="indicators">The indicators value.</param>
        public void UpsertIndicators(string symbol, Market.Strategy.Indicators indicators)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
            ArgumentNullException.ThrowIfNull(indicators);

            lock (_indicatorsGate)
            {
                string key = NormalizeSymbol(symbol);

                if (!_indicators.TryGetValue(key, out var series))
                {
                    series = [];
                    _indicators[key] = series;
                }

                series[indicators.TimestampUtc] = CloneIndicators(indicators);

                while (series.Count > _maxIndicatorsPerSeries)
                {
                    DateTime oldest = series.First().Key;
                    series.Remove(oldest);
                }
            }
        }

        /// <summary>
        /// Executes the get indicators operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <returns>The get indicators result.</returns>
        public IReadOnlyList<Market.Strategy.Indicators> GetIndicators(string symbol)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

            lock (_indicatorsGate)
            {
                string key = NormalizeSymbol(symbol);

                if (!_indicators.TryGetValue(key, out var series))
                    return [];

                return [.. series.Values.Select(CloneIndicators)];
            }
        }

        /// <summary>
        /// Executes the upsert signal operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="signal">The signal value.</param>
        public void UpsertSignal(string symbol, Market.Strategy.Signal signal)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
            ArgumentNullException.ThrowIfNull(signal);

            lock (_signalsGate)
            {
                string key = NormalizeSymbol(symbol);

                if (!_signals.TryGetValue(key, out var series))
                {
                    series = [];
                    _signals[key] = series;
                }

                series[signal.TimestampUtc] = CloneSignal(signal);

                while (series.Count > _maxSignalsPerSeries)
                {
                    DateTime oldest = series.First().Key;
                    series.Remove(oldest);
                }
            }
        }

        /// <summary>
        /// Executes the get signals operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <returns>The get signals result.</returns>
        public IReadOnlyList<Market.Strategy.Signal> GetSignals(string symbol)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

            lock (_signalsGate)
            {
                string key = NormalizeSymbol(symbol);

                if (!_signals.TryGetValue(key, out var series))
                    return [];

                return [.. series.Values.Select(CloneSignal)];
            }
        }

        /// <summary>
        /// Executes the upsert kline operation.
        /// </summary>
        /// <param name="kline">The kline value.</param>
        /// <returns>The upsert kline result.</returns>
        public KlineUpsertResult UpsertKline(Kline kline)
        {
            ArgumentNullException.ThrowIfNull(kline);

            lock (_klinesGate)
            {
                string symbol = NormalizeSymbol(kline.Symbol!);
                var key = (symbol, kline.Interval);

                if (!_klines.TryGetValue(key, out var series))
                {
                    series = [];
                    _klines[key] = series;
                }

                bool existed = series.TryGetValue(kline.OpenTime, out Kline? existing);

                series[kline.OpenTime] = CloneKline(kline);

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
                        current: CloneKline(kline));
                }

                bool updated = !AreEquivalent(existing!, kline);

                return new KlineUpsertResult(
                    inserted: false,
                    updated: updated,
                    previous: existing == null ? null : CloneKline(existing),
                    current: CloneKline(kline));
            }
        }

        /// <summary>
        /// Executes the get klines operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <param name="interval">The interval value.</param>
        /// <returns>The get klines result.</returns>
        public IReadOnlyList<Kline> GetKlines(string symbol, KlineInterval interval)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

            lock (_klinesGate)
            {
                var key = (NormalizeSymbol(symbol), interval);

                if (!_klines.TryGetValue(key, out var series))
                    return [];

                return [.. series.Values.Select(CloneKline)];
            }
        }

        /// <summary>
        /// Executes the add trade operation.
        /// </summary>
        /// <param name="trade">The trade value.</param>
        /// <returns>The add trade result.</returns>
        public bool AddTrade(Trade trade)
        {
            ArgumentNullException.ThrowIfNull(trade);

            lock (_tradesGate)
            {
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

                trades.AddLast(CloneTrade(trade));

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
        }

        /// <summary>
        /// Executes the get trades operation.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <returns>The get trades result.</returns>
        public IReadOnlyList<Trade> GetTrades(string symbol)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

            lock (_tradesGate)
            {
                symbol = NormalizeSymbol(symbol);

                if (!_trades.TryGetValue(symbol, out var trades))
                    return [];

                return [.. trades.Select(CloneTrade)];
            }
        }

        private sealed class SymbolComparer : IEqualityComparer<Symbol>
        {
            public static readonly SymbolComparer Instance = new();

            /// <summary>
            /// Executes the equals operation.
            /// </summary>
            /// <param name="x">The x value.</param>
            /// <param name="y">The y value.</param>
            /// <returns>The equals result.</returns>
            public bool Equals(Symbol? x, Symbol? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;

                return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Executes the get hash code operation.
            /// </summary>
            /// <param name="obj">The obj value.</param>
            /// <returns>The get hash code result.</returns>
            public int GetHashCode(Symbol obj)
            {
                return obj.Name?.ToUpperInvariant().GetHashCode() ?? 0;
            }
        }

        private static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant();

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

        private static Kline CloneKline(Kline source)
        {
            return new Kline
            {
                Symbol = source.Symbol,
                Exchange = source.Exchange,
                Interval = source.Interval,
                OpenTime = source.OpenTime,
                CloseTime = source.CloseTime,
                Open = source.Open,
                High = source.High,
                Low = source.Low,
                Close = source.Close,
                Volume = source.Volume,
                NumberOfTrades = source.NumberOfTrades,
                QuoteAssetVolume = source.QuoteAssetVolume,
                TakerBuyQuoteAssetVolume = source.TakerBuyQuoteAssetVolume,
                TakerBuyBaseAssetVolume = source.TakerBuyBaseAssetVolume,
                Final = source.Final
            };
        }

        private static Trade CloneTrade(Trade source)
        {
            return new Trade
            {
                Symbol = source.Symbol,
                Exchange = source.Exchange,
                Id = source.Id,
                Time = source.Time,
                Price = source.Price,
                BaseQuantity = source.BaseQuantity,
                QuoteQuantity = source.QuoteQuantity
            };
        }

        private static Market.Strategy.Indicators CloneIndicators(Market.Strategy.Indicators source)
        {
            return new Market.Strategy.Indicators
            {
                TimestampUtc = source.TimestampUtc,
                Values = new Dictionary<string, decimal>(source.Values)
            };
        }

        private static Market.Strategy.Signal CloneSignal(Market.Strategy.Signal source)
        {
            return new Market.Strategy.Signal
            {
                TimestampUtc = source.TimestampUtc,
                SignalType = source.SignalType,
                Reason = source.Reason
            };
        }
    }
}
