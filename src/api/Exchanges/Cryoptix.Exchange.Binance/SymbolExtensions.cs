using Binance.Net.Objects.Models.Spot;
using Cryoptix.Market.Data;

namespace Cryoptix.Exchange.Binance
{
    public static class SymbolExtensions
    {
        public static Symbol ToCryoptixSymbol(this BinanceSymbol s)
        {
            BinanceSymbolPriceFilter? priceFilter = s.Filters.OfType<BinanceSymbolPriceFilter>().FirstOrDefault();
            BinanceSymbolLotSizeFilter? lotSizeFilter = s.Filters.OfType<BinanceSymbolLotSizeFilter>().FirstOrDefault();
            BinanceSymbolNotionalFilter? notionalFilter = s.Filters.OfType<BinanceSymbolNotionalFilter>().FirstOrDefault();

            Symbol symbol = new()
            {
                Name = $"{s.BaseAsset}/{s.QuoteAsset}",
                ExchangeSymbol = $"{s.BaseAsset}{s.QuoteAsset}",
                Exchange = Market.Data.Exchange.Binance,
                BaseAsset = s.BaseAsset, 
                BaseAssetPrecision = s.BaseAssetPrecision,
                QuoteAsset = s.QuoteAsset, 
                QuoteAssetPrecision = s.QuoteAssetPrecision,
                NotionalMinimumValue = notionalFilter != null ? notionalFilter.MinNotional : 0m,
                TickSize = priceFilter != null ? priceFilter.TickSize : 0m,
                LotSize = lotSizeFilter != null ? lotSizeFilter.StepSize : 0m
            };

            return symbol;
        }
    }
}
