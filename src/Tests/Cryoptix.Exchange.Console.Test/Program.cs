// See https://aka.ms/new-console-template for more information
using Binance.Net.Clients;
using Cryoptix.Core.Enums;
using Cryoptix.Core.Models;
using Cryoptix.Exchange.Binance;
using Cryoptix.Exchange.Console.Test;

Console.WriteLine("Cryoptix.ExchangeApi.Console.Test");
Console.WriteLine();

using CancellationTokenSource cts = new();
CancellationToken ct = cts.Token;
Credentials credentials = new() 
{
    AccountName = "Test Account",
    ApiKey = "your_api",
    ApiSecret = "your_secret"
};

ClientOrder clientOrder = new()
{
    Symbol = "BTCUSDC",
    Side = OrderSide.Buy,
    Type = OrderType.Limit,
    TimeInForce = TimeInForce.GTC,
    Quantity = 1m,
    Price = 50000m
};

#pragma warning disable CS0219 // Variable is assigned but its value is never used
string btcUsdc = "BTCUSDC";
string ethUSDT = "ETHUSDT";
int? limit = 100;
int? level = 10;
int? recWindow = null;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

TestExchangeRestApi exchangeRestApi = new(new BinanceRestApi(new BinanceRestClient(), credentials), credentials.AccountName);
//_ = await exchangeRestApi.CancelOrderAsync(btcUsdc, "123", ct);
//_ = await exchangeRestApi.GetAccountInfoAsync(ct);
//_ = await exchangeRestApi.GetOpenOrdersAsync(btcUsdc, recWindow, ct);
//_ = await exchangeRestApi.PlaceOrderAsync(clientOrder, recWindow, ct);
//_ = await exchangeRestApi.GetKlinesAsync(btcUsdc, KlineInterval.Minute, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, limit, ct);
//_ = await exchangeRestApi.GetOrderBookAsync(btcUsdc, limit, ct);
//_ = await exchangeRestApi.GetSymbolsAsync(ct);
_ = await exchangeRestApi.GetTradesAsync(btcUsdc, limit, ct);
exchangeRestApi.Dispose();

TestExchangeSubscriptionApi exchangeSubscriptionApi = new(new BinanceSubscriptionApi());
//await using IAsyncDisposable subscribeAccount = await exchangeSubscriptionApi.SubscribeToAccountUpdatesAsync(credentials, o => { }, e => { }, ct);
//await using IAsyncDisposable subscribeKlines = await exchangeSubscriptionApi.SubscribeToKlineUpdatesAsync(btcUsdc, KlineInterval.Minute, k => { }, e => { }, ct);
//await using IAsyncDisposable subscribeOrderBook = await exchangeSubscriptionApi.SubscribeToOrderBookAsync(btcUsdc, level, o => { }, e => { }, ct);
//await using IAsyncDisposable subscribeStatistics = await exchangeSubscriptionApi.SubscribeToSymbolStatisticsAsync([btcUsdc, ethUSDT], s => { }, e => { }, ct);
await using IAsyncDisposable subscribeTrades = await exchangeSubscriptionApi.SubscribeToTradesAsync(btcUsdc, t => { }, e => { }, ct);

_ = Console.ReadLine();

cts.Cancel();

Console.ReadLine();

