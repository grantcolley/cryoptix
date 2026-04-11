namespace Cryoptix.Web.API.Config
{
    public static class ConfigKeys
    {
        public const string AUTH_DOMAIN = "Auth:Domain";
        public const string AUTH_AUDIENCE = "Auth:Audience";
        public const string AUTH_ISSUER = "Auth:Issuer";
        public const string CORS_POLICY = "CorsOrigins:Policy";
        public const string CORS_ORIGINS_URLS = "CorsOrigins:Urls";
        public const string STRATEGY_CHANNEL_OPTIONS_KLINE_CAPACITY = "StrategyChannelOptions:KlineCapacity";
        public const string STRATEGY_CHANNEL_OPTIONS_TRADE_CAPACITY = "StrategyChannelOptions:TradeCapacity";
        public const string STRATEGY_CHANNEL_OPTIONS_DROP_TRADES_WHEN_FULL = "StrategyChannelOptions:DropTradesWhenFull";
        public const string STRATEGY_CHANNEL_OPTIONS_KLINE_FULL_MODE = "StrategyChannelOptions:KlineFullMode";
    }
}
