using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Binance;
using Cryoptix.Market.Data;
using Cryoptix.Observer.Authorization;
using Cryoptix.Observer.Metrics;
using Cryoptix.Observer.Notification;
using Cryoptix.Observer.Subscription;
using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Cache;
using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Controller;
using Cryoptix.Strategy.Dispatcher;
using Cryoptix.Strategy.Engine;
using Cryoptix.Strategy.Engine.MovingAverage;
using Cryoptix.Strategy.Notification;
using Cryoptix.Strategy.Order;
using Cryoptix.Strategy.Processor;
using Cryoptix.Strategy.Seeding;
using Cryoptix.Strategy.Signal;
using Cryoptix.Strategy.State;
using Cryoptix.Strategy.Subscription;
using Cryoptix.Web.API.Authorization;
using Cryoptix.Web.API.Config;
using Cryoptix.Web.API.Constants;
using Cryoptix.Web.API.Endpoints;
using Cryoptix.Web.API.ExceptionHandling;
using Cryoptix.Web.API.Notification;
using Cryoptix.Web.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Threading.Channels;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

string klineCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_CAPACITY);
string tradeCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_CAPACITY);
string tradeFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_FULL_MODE);
string klineFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_FULL_MODE);

string klineBroadcastCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_BROADCAST_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_BROADCAST_CAPACITY);
string tradeBroadcastCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_BROADCAST_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_BROADCAST_CAPACITY);
string indicatorsBroadcastCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_INDICATORS_BROADCAST_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_INDICATORS_BROADCAST_CAPACITY);
string signalBroadcastCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_SIGNAL_BROADCAST_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_SIGNAL_BROADCAST_CAPACITY);
string klineBroadcastFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_BROADCAST_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_BROADCAST_FULL_MODE);
string tradeBroadcastFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_BROADCAST_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_BROADCAST_FULL_MODE);
string indicatorsBroadcastFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_INDICATORS_BROADCAST_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_INDICATORS_BROADCAST_FULL_MODE);
string signalBroadcastFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_SIGNAL_BROADCAST_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_SIGNAL_BROADCAST_FULL_MODE);

string domain = builder.Configuration[ConfigKeys.AUTH_DOMAIN] ?? throw new NullReferenceException(ConfigKeys.AUTH_DOMAIN);
string audience = builder.Configuration[ConfigKeys.AUTH_AUDIENCE] ?? throw new NullReferenceException(ConfigKeys.AUTH_AUDIENCE);
string issuer = builder.Configuration[ConfigKeys.AUTH_ISSUER] ?? throw new NullReferenceException(ConfigKeys.AUTH_ISSUER);

string corsPolicy = builder.Configuration[ConfigKeys.CORS_POLICY] ?? throw new NullReferenceException(ConfigKeys.CORS_POLICY);
string corsOriginUrls = builder.Configuration[ConfigKeys.CORS_ORIGINS_URLS] ?? throw new NullReferenceException(ConfigKeys.CORS_ORIGINS_URLS);

builder.Services.Configure<AuthOptions>(builder.Configuration.GetRequiredSection("Auth"));

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .WriteTo.Console();
});

// Add services to the container.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = domain;
        options.Audience = audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            NameClaimType = "sub",
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/api/strategy/subscribe"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Claims.CRYOPTIX_CLIENT_ID, policy =>
    {
        policy.AddRequirements(new AllowedClientRequirement());
    })
    .AddPolicy(Claims.CRYOPTIX_USER_CLAIM, policy =>
    {
        policy.RequireAuthenticatedUser().RequireClaim("permissions", Claims.CRYOPTIX_USER_CLAIM);
    })
    .AddPolicy(Claims.CRYOPTIX_DEVELOPER_CLAIM, policy =>
    {
        policy.RequireAuthenticatedUser().RequireClaim("permissions", Claims.CRYOPTIX_DEVELOPER_CLAIM);
    });

if (!string.IsNullOrWhiteSpace(corsPolicy)
    && !string.IsNullOrWhiteSpace(corsOriginUrls))
{
    builder.Services.AddCors(options =>
    {
        string[] urls = corsOriginUrls.Split(';');

        options.AddPolicy(corsPolicy,
            builder =>
                builder.WithOrigins(urls)
                .AllowAnyHeader()
                .WithMethods("GET", "POST", "OPTIONS")
                .AllowCredentials());
    });
}

builder.Services.AddApiExceptionHandling();

builder.Services.AddSingleton<IAuthorizationHandler, AllowedClientHandler>();

builder.Services.AddSingleton<Credentials>(
    builder.Configuration.GetRequiredSection("Credentials").Get<Credentials>()
    ?? throw new NullReferenceException("BinanceApi credentials not found in configuration"));

builder.Services.AddHostedService<StrategyBackgroundService>();

Channel<StrategyCommand> strategyCommandChannel = Channel.CreateBounded<StrategyCommand>(
    new BoundedChannelOptions(100)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });

builder.Services.AddSingleton(strategyCommandChannel);
builder.Services.AddSingleton(strategyCommandChannel.Reader);
builder.Services.AddSingleton(strategyCommandChannel.Writer);
builder.Services.AddSingleton(new StrategyChannelOptions
{
    KlineCapacity = Int32.Parse(klineCapacity),
    TradeCapacity = Int32.Parse(tradeCapacity),
    TradeFullMode = (BoundedChannelFullMode)int.Parse(tradeFullMode),
    KlineFullMode = (BoundedChannelFullMode)int.Parse(klineFullMode),
    KlineBroadcastCapacity = Int32.Parse(klineBroadcastCapacity),
    TradeBroadcastCapacity = Int32.Parse(tradeBroadcastCapacity),
    KlineBroadcastFullMode = (BoundedChannelFullMode)int.Parse(klineBroadcastFullMode),
    TradeBroadcastFullMode = (BoundedChannelFullMode)int.Parse(tradeBroadcastFullMode),
    IndicatorsBroadcastCapacity = Int32.Parse(indicatorsBroadcastCapacity),
    SignalBroadcastCapacity = Int32.Parse(signalBroadcastCapacity),
    IndicatorsBroadcastFullMode = (BoundedChannelFullMode)int.Parse(indicatorsBroadcastFullMode),
    SignalBroadcastFullMode = (BoundedChannelFullMode)int.Parse(signalBroadcastFullMode)
});

builder.Services.AddSignalR();

builder.Services.AddSingleton<IUserContextAccessor, Auth0UserContextAccessor>();
builder.Services.AddSingleton<ISubscriptionManager, InMemorySubscriptionManager>();
builder.Services.AddSingleton<INotificationBroadcaster, SignalRNotificationBroadcaster>();
builder.Services.AddSingleton<INotificationMetrics, NotificationMetrics>();
builder.Services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddSingleton<INotificationPump, NotificationPump>();
builder.Services.AddSingleton<IBinanceRestClient, BinanceRestClient>();
builder.Services.AddSingleton<IExchangeRestApi, BinanceRestApi>();
builder.Services.AddSingleton<IExchangeSubscriptionApi, BinanceSubscriptionApi>();
builder.Services.AddSingleton<IExchangeApiFactory, ExchangeApiFactory>();
builder.Services.AddSingleton<StrategyStateStore>();
builder.Services.AddSingleton<IStrategyCommandQueue, StrategyCommandQueue>();
builder.Services.AddSingleton<IStrategyController, StrategyController>();
builder.Services.AddSingleton<IStrategyAgent, StrategyAgent>();
builder.Services.AddSingleton<IStrategyClock, SystemStrategyClock>();
builder.Services.AddSingleton<IStrategyMarketSeeder, StrategyMarketSeeder>();
builder.Services.AddSingleton<ITradingFlowSessionAccessor, TradingFlowSessionAccessor>();
builder.Services.AddSingleton<IMarketDataSnapshotProvider, MarketDataSnapshotProvider>();
builder.Services.AddSingleton<IStrategyStatusNotifier, StrategyStatusNotifier>();
builder.Services.AddSingleton<IStrategyMarketEventSubscriber, StrategyMarketEventSubscriber>();
builder.Services.AddSingleton<IStrategyAnalysisContextFactory, StrategyAnalysisContextFactory>();
builder.Services.AddSingleton<IOrderSizingService, OrderSizingService>();
builder.Services.AddSingleton<IOrderExecutionService, OrderExecutionService>();
builder.Services.AddSingleton<IStrategySignalHandler, StrategySignalHandler>();
builder.Services.AddSingleton<IStrategyMarketEventDispatcher, StrategyMarketEventDispatcher>();
builder.Services.AddSingleton<IStrategyEventChannelFactory, StrategyEventChannelFactory>();
builder.Services.AddSingleton<MovingAverageIndicatorEngine>();
builder.Services.AddSingleton<MovingAverageSignalEngine>();
builder.Services.AddSingleton<IStrategyEnginePair, MovingAverageStrategyEnginePair>();
builder.Services.AddSingleton<IStrategyEnginePairFactory, StrategyEnginePairFactory>();
builder.Services.AddTransient<TradingFlowProcessor>();
builder.Services.AddSingleton<IStrategyProcessorCatalog>(sp =>
    new StrategyProcessorCatalog(
    [
        new KeyValuePair<StrategyProcessorType, Func<IStrategyProcessor>>(StrategyProcessorType.TradingFlow, () => sp.GetRequiredService<TradingFlowProcessor>())
    ]));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiExceptionHandling();

if (!string.IsNullOrWhiteSpace(corsPolicy))
{
    app.UseCors(corsPolicy);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCryoptixApi();

app.Run();