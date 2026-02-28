using Binance.Net.Objects.Models.Spot;
using Cryoptix.Core.Enums;
using Cryoptix.Core.Models;

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
                Name = $"{s.BaseAsset}{s.QuoteAsset}",
                ExchangeSymbol = $"{s.BaseAsset}{s.QuoteAsset}",
                Exchange = Core.Enums.Exchange.Binance,
                BaseAsset = new Asset { Symbol = s.BaseAsset, Precision = s.BaseAssetPrecision },
                QuoteAsset = new Asset { Symbol = s.QuoteAsset, Precision = s.QuoteAssetPrecision },
                OrderTypes = [OrderType.Limit, OrderType.Market, OrderType.StopLoss, OrderType.StopLossLimit, OrderType.TakeProfit, OrderType.TakeProfitLimit],
                NotionalMinimumValue = notionalFilter != null ? notionalFilter.MinNotional : 0m,
                TickSize = priceFilter != null ? priceFilter.TickSize : 0m,
                LotSize = lotSizeFilter != null ? lotSizeFilter.StepSize : 0m
            };

            return symbol;
        }
    }
}
