using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Cryoptix.Exchange.Api;
using Cryoptix.Exchange.Binance;
using Cryoptix.Exchange.Models;
using Cryoptix.Strategy.Agent;
using Cryoptix.Strategy.Analysis;
using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Channel;
using Cryoptix.Strategy.Clock;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Controller;
using Cryoptix.Strategy.Dispatcher;
using Cryoptix.Strategy.Engine;
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

string klineCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_CAPACITY);
string tradeCapacity = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_CAPACITY] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_TRADE_CAPACITY);
string dropTradesWhenFull = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_DROP_TRADES_WHEN_FULL] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_DROP_TRADES_WHEN_FULL);
string klineFullMode = builder.Configuration[ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_FULL_MODE] ?? throw new NullReferenceException(ConfigKeys.STRATEGY_CHANNEL_OPTIONS_KLINE_FULL_MODE);

string domain = builder.Configuration[ConfigKeys.AUTH_DOMAIN] ?? throw new NullReferenceException(ConfigKeys.AUTH_DOMAIN);
string audience = builder.Configuration[ConfigKeys.AUTH_AUDIENCE] ?? throw new NullReferenceException(ConfigKeys.AUTH_AUDIENCE);
string issuer = builder.Configuration[ConfigKeys.AUTH_ISSUER] ?? throw new NullReferenceException(ConfigKeys.AUTH_ISSUER);

builder.Services.Configure<AuthOptions>(builder.Configuration.GetRequiredSection("Auth"));

builder.Host.UseSerilog((ctx, lc) =>
{
    // %HOME% exists on App Service (Windows + Linux). On Windows it maps to D:\home.
    var home = Environment.GetEnvironmentVariable("HOME") ?? "";
    var logDir = Path.Combine(home, "LogFiles", "Application");
    Directory.CreateDirectory(logDir);

    lc.ReadFrom.Configuration(ctx.Configuration)
      .WriteTo.Console()
      .WriteTo.File(
          path: Path.Combine(logDir, "cryoptix-.log"),
          rollingInterval: RollingInterval.Day,
          retainedFileCountLimit: 7,
          shared: true);
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

builder.Services.AddSingleton(new StrategyChannelOptions
{
    KlineCapacity = Int32.Parse(klineCapacity),
    TradeCapacity = Int32.Parse(tradeCapacity),
    DropTradesWhenFull = bool.Parse(dropTradesWhenFull),
    KlineFullMode = (BoundedChannelFullMode)int.Parse(klineFullMode)
});

builder.Services.AddSingleton(strategyCommandChannel);
builder.Services.AddSingleton(strategyCommandChannel.Reader);
builder.Services.AddSingleton(strategyCommandChannel.Writer);
builder.Services.AddSingleton<IBinanceRestClient, BinanceRestClient>();
builder.Services.AddSingleton<IExchangeRestApi, BinanceRestApi>();
builder.Services.AddSingleton<IExchangeSubscriptionApi, BinanceSubscriptionApi>();
builder.Services.AddSingleton<IExchangeApiFactory, ExchangeApiFactory>();
builder.Services.AddSingleton<StrategyStateStore>();
builder.Services.AddSingleton<IStrategyCommandQueue, StrategyCommandQueue>();
builder.Services.AddSingleton<IStrategyController, StrategyController>();
builder.Services.AddSingleton<IStrategyAgent, StrategyAgent>();
builder.Services.AddSingleton<IStrategyClock, SystemStrategyClock>();
builder.Services.AddSingleton<IStrategyEventChannelFactory, StrategyEventChannelFactory>();
builder.Services.AddSingleton<IStrategyMarketSeeder, StrategyMarketSeeder>();
builder.Services.AddSingleton<IStrategyMarketEventSubscriber, StrategyMarketEventSubscriber>();
builder.Services.AddSingleton<IStrategySignalHandler, StrategySignalHandler>();
builder.Services.AddSingleton<IStrategyMarketEventDispatcher, StrategyMarketEventDispatcher>();
builder.Services.AddSingleton<IStrategyAnalysisContextFactory, StrategyAnalysisContextFactory>();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCryoptixApi();

app.Run();