using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Cryoptix.Core.Api;
using Cryoptix.Core.Models;
using Cryoptix.Exchange.Binance;
using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Command;
using Cryoptix.Strategy.Controller;
using Cryoptix.Strategy.Execution;
using Cryoptix.Strategy.Status;
using Cryoptix.Strategy.Strategies;
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

Channel<StrategyCommand> channel = Channel.CreateBounded<StrategyCommand>(
    new BoundedChannelOptions(100)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });

builder.Services.AddSingleton(channel);
builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);

builder.Services.AddSingleton<IExchangeRestApi, BinanceRestApi>();
builder.Services.AddSingleton<IExchangeSubscriptionApi, BinanceSubscriptionApi>();
builder.Services.AddSingleton<IExchangeApiFactory, ExchangeApiFactory>();
builder.Services.AddSingleton<StrategyStateStore>();
builder.Services.AddSingleton<IStrategyCommandQueue, StrategyCommandQueue>();
builder.Services.AddSingleton<IStrategyController, StrategyController>();
builder.Services.AddSingleton<IBinanceRestClient, BinanceRestClient>();
builder.Services.AddSingleton<IExchangeRestApi, BinanceRestApi>();
builder.Services.AddSingleton<IExchangeSubscriptionApi, BinanceSubscriptionApi>();
builder.Services.AddSingleton<IStrategyExecution, StrategyExecution>();
builder.Services.AddTransient<MovingAverage>();
builder.Services.AddSingleton<IStrategyCatalog>(sp =>
    new StrategyCatalog(
    [
        new KeyValuePair<StrategyType, Func<IStrategyExecutable>>(StrategyType.MovingAverage, () => sp.GetRequiredService<MovingAverage>())
    ]));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCryoptixApi();

app.Run();