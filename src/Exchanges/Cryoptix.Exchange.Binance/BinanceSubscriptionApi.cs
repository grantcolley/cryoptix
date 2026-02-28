using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;
using Binance.Net.SymbolOrderBooks;
using Cryoptix.Core.Enums;
using Cryoptix.Core.Interfaces;
using Cryoptix.Core.Models;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System.Data;

namespace Cryoptix.Exchange.Binance
{
    public sealed class BinanceSubscriptionApi : IExchangeSubscriptionApi
    {
        private const Core.Enums.Exchange Exchange = Core.Enums.Exchange.Binance;

        public async Task<IAsyncDisposable> SubscribeToAccountUpdatesAsync(Credentials credentials, Action<AccountEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(credentials.AccountName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(credentials.ApiKey);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(credentials.ApiSecret);
            ArgumentNullException.ThrowIfNull(onCallback);
            ArgumentNullException.ThrowIfNull(onError);

            CancellationTokenRegistration cancellationTokenRegistration = default;
            UpdateSubscription? updateSubscription = null;

            // NOTE: For Spot WS user-data in recent Binance.Net versions, Binance requires Ed25519 keys.
            // If your `ApiSecret` is not an Ed25519 private key, authentication may fail.
            ApiCredentials apiCredentials = new(credentials.ApiKey, credentials.ApiSecret);

            // IMPORTANT: do NOT 'using' this. The returned handle owns it.
            BinanceSocketClient socketClient = new (options =>
            {
                options.ApiCredentials = apiCredentials;
            });

            // We'll create the handle early, but it will only be "armed" once subscription is set.
            AsyncSubscription asyncSubscriptionHandle = new(async () =>
            {
                // stop listening to token
                cancellationTokenRegistration.Dispose();

                // close WS subscription, then dispose client
                if (updateSubscription != null)
                {
                    try { await updateSubscription.CloseAsync().ConfigureAwait(false); }
                    catch (Exception ex) { SafeOnError(ex, onError); }
                }

                try { socketClient.Dispose(); }
                catch (Exception ex) { SafeOnError(ex, onError); }
            });

            try
            {
                CallResult<UpdateSubscription> result = await socketClient.SpotApi.Account.SubscribeToUserDataUpdatesAsync(
                    onAccountPositionMessage: data =>
                    {
                        try
                        {
                            // BinanceStreamPositionsUpdate provides balance snapshots
                            BinanceStreamPositionsUpdate positionUpdate = data.Data;

                            Account account = new()
                            {
                                Name = credentials.AccountName,
                                Exchange = Exchange,
                                Time = positionUpdate.EventTime
                            };

                            foreach (BinanceStreamBalance b in positionUpdate.Balances.Where(b => b.Available > 0 || b.Locked > 0))
                            {
                                account.Balances.Add(new Balance
                                {
                                    Asset = b.Asset,
                                    Free = b.Available,
                                    Locked = b.Locked
                                });
                            }

                            SafeOnCallback(new AccountEventArgs { Account = account }, onCallback, onError);
                        }
                        catch (Exception ex)
                        {
                            SafeOnError(ex, onError);
                        }
                    },
                    ct: cancellationToken
                ).ConfigureAwait(false);

                if (!result.Success)
                {
                    throw new Exception($"SubscribeToAccountUpdatesAsync({credentials.AccountName}): {result.Error?.ToExchangeErrorMessage()}", result.Error?.Exception);
                }

                updateSubscription = result.Data;

                if (cancellationToken.IsCancellationRequested)
                {
                    await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false);
                    throw new OperationCanceledException(cancellationToken);
                }

                // Tie cancellation to disposal (sync callback; observe exceptions)
                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(() =>
                    {
                        _ = Task.Run(async () =>
                        {
                            try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); }
                            catch (Exception ex) { SafeOnError(ex, onError); }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                // If subscribe fails, clean up now, then rethrow
                try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); } catch { /* ignore */ }
                SafeOnError(ex, onError);
                throw;
            }

            // Return immediately; caller controls lifetime via handle.DisposeAsync()
            return asyncSubscriptionHandle;
        }

        public async Task<IAsyncDisposable> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<KlineEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
            ArgumentNullException.ThrowIfNull(onCallback);
            ArgumentNullException.ThrowIfNull(onError);

            global::Binance.Net.Enums.KlineInterval klineInterval = interval.ToKlineInterval();

            // IMPORTANT: do NOT 'using' this. The returned handle owns it.
            BinanceSocketClient socketClient = new();
            UpdateSubscription? updateSubscription = null;
            CancellationTokenRegistration cancellationTokenRegistration = default;

            AsyncSubscription asyncSubscriptionHandle = new(async () =>
            {
                // stop listening to token
                cancellationTokenRegistration.Dispose();

                // close subscription, then dispose client
                if (updateSubscription != null)
                {
                    try { await updateSubscription.CloseAsync().ConfigureAwait(false); }
                    catch (Exception ex) { SafeOnError(ex, onError); }
                }

                try { socketClient.Dispose(); }
                catch (Exception ex) { SafeOnError(ex, onError); }
            });

            try
            {
                CallResult<UpdateSubscription> result = await socketClient.SpotApi.ExchangeData
                    .SubscribeToKlineUpdatesAsync(
                        symbol,
                        klineInterval,
                        data =>
                        {
                            try
                            {
                                IBinanceStreamKlineData kData = data.Data;
                                IBinanceStreamKline k = kData.Data;

                                Kline kline = new()
                                {
                                    Interval = interval,
                                    Exchange = Exchange,
                                    Symbol = kData.Symbol,
                                    OpenTime = k.OpenTime,
                                    CloseTime = k.CloseTime,
                                    Open = k.OpenPrice,
                                    High = k.HighPrice,
                                    Low = k.LowPrice,
                                    Close = k.ClosePrice,
                                    Volume = k.Volume,
                                    NumberOfTrades = k.TradeCount,
                                    QuoteAssetVolume = k.QuoteVolume,
                                    TakerBuyQuoteAssetVolume = k.TakerBuyQuoteVolume,
                                    TakerBuyBaseAssetVolume = k.TakerBuyBaseVolume,
                                    Final = k.Final,
                                };

                                SafeOnCallback(new KlineEventArgs { Klines = [kline] }, onCallback, onError);
                            }
                            catch (Exception ex)
                            {
                                SafeOnError(ex, onError);
                            }
                        },
                        ct: cancellationToken
                    )
                    .ConfigureAwait(false);

                if (!result.Success)
                {
                    throw new Exception($"SubscribeToKlineUpdatesAsync({symbol}, {interval}) {result.Error?.ToExchangeErrorMessage()}", result.Error?.Exception);
                }

                updateSubscription = result.Data;

                if (cancellationToken.IsCancellationRequested)
                {
                    await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false);
                    throw new OperationCanceledException(cancellationToken);
                }

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(() =>
                    {
                        _ = Task.Run(async () =>
                        {
                            try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); }
                            catch (Exception ex) { SafeOnError(ex, onError); }
                        });
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Already disposed above
                throw;
            }
            catch (Exception ex)
            {
                // If subscribe fails, clean up what we created and rethrow
                try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); } catch { /* ignore */ }
                SafeOnError(ex, onError);
                throw;
            }

            return asyncSubscriptionHandle;
        }

        public async Task<IAsyncDisposable> SubscribeToOrderBookAsync(string symbol, int? limit, Action<OrderBookEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
            ArgumentNullException.ThrowIfNull(onCallback);
            ArgumentNullException.ThrowIfNull(onError);

            CancellationTokenRegistration cancellationTokenRegistration = default;

            // This class maintains a correct local book (snapshot + diffs)
            BinanceSpotSymbolOrderBook book = new(symbol, options =>
            {
                options.Limit = limit;
            });

            void Publish()
            {
                try
                {
                    (ISymbolOrderBookEntry[] bids, ISymbolOrderBookEntry[] asks) = book.Book; // local state (bids/asks levels)

                    OrderBook orderBook = new()
                    {
                        Symbol = symbol,
                        Exchange = Exchange,
                        LastUpdateId = book.LastSequenceNumber,
                        UpdateTime = book.UpdateTime,
                        BestAsk = new OrderBookPrice { Price = book.BestAsk.Price, Quantity = book.BestAsk.Quantity },
                        BestBid = new OrderBookPrice { Price = book.BestBid.Price, Quantity = book.BestBid.Quantity },
                        Asks = [.. asks.Select(x => new OrderBookPrice { Price = x.Price, Quantity = x.Quantity })],
                        Bids = [.. bids.Select(x => new OrderBookPrice { Price = x.Price, Quantity = x.Quantity })]
                    };

                    SafeOnCallback(new OrderBookEventArgs { OrderBook = orderBook }, onCallback, onError);
                }
                catch (Exception ex)
                {
                    SafeOnError(ex, onError);
                }
            }

            // IMPORTANT: keep a reference so we can unsubscribe later
            void handler((ISymbolOrderBookEntry[] Bids, ISymbolOrderBookEntry[] Asks) _) => Publish();

            // Called when the local book changes
            book.OnOrderBookUpdate += handler;

            try
            {
                // Start syncing (downloads snapshot + subscribes to diffs)
                CallResult<bool> result = await book.StartAsync().ConfigureAwait(false);

                if (!result)
                {
                    throw new Exception($"SubscribeToOrderBookAsync({symbol}, {limit}) {result.Error?.ToExchangeErrorMessage()}", result.Error?.Exception);
                }

                // Publish an initial snapshot immediately
                Publish();
            }
            catch (Exception ex)
            {
                // If start fails, clean up now and rethrow (or call onError then throw)
                try { book.OnOrderBookUpdate -= handler; } catch { }
                try { await book.StopAsync().ConfigureAwait(false); } catch { }
                book.Dispose();

                SafeOnError(ex, onError);
                throw;
            }

            // Return a handle that the consumer disposes to stop the subscription.
            AsyncSubscription asyncSubscriptionHandle = new(async () =>
            {
                cancellationTokenRegistration.Dispose(); // stop listening to token

                // make this idempotent if you want to be extra safe
                try { book.OnOrderBookUpdate -= handler; } catch { }
                try { await book.StopAsync().ConfigureAwait(false); } catch { }
                book.Dispose();
            });

            if (cancellationToken.IsCancellationRequested)
            {
                await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false);
                throw new OperationCanceledException(cancellationToken);
            }

            if (cancellationToken.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellationToken.Register(() =>
                {
                    _ = Task.Run(async () =>
                    {
                        try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); }
                        catch (Exception ex) { SafeOnError(ex, onError); }
                    });
                });
            }

            return asyncSubscriptionHandle;
        }

        public async Task<IAsyncDisposable> SubscribeToSymbolStatisticsAsync(IEnumerable<string> symbols, Action<StatisticsEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(symbols);
            ArgumentNullException.ThrowIfNull(onCallback);
            ArgumentNullException.ThrowIfNull(onError);

            List<string> list = [.. symbols.Where(s => !string.IsNullOrWhiteSpace(s))
                              .Select(s => s.Trim().ToUpperInvariant())
                              .Distinct()];

            if (list.Count == 0) throw new ArgumentException("No symbols provided", nameof(symbols));

            // IMPORTANT: do NOT 'using' this. The returned handle owns it.
            BinanceSocketClient socketClient = new();
            List<UpdateSubscription> updateSubscriptions = new(capacity: list.Count);
            CancellationTokenRegistration cancellationTokenRegistration = default;

            // Create the handle up front; it will close what was successfully opened.
            AsyncSubscription asyncSubscriptionHandle = new(async () =>
            {
                // Stop listening to token
                cancellationTokenRegistration.Dispose();

                // Close all subs (best-effort)
                foreach (UpdateSubscription s in updateSubscriptions)
                {
                    try { await s.CloseAsync().ConfigureAwait(false); }
                    catch (Exception ex) { SafeOnError(ex, onError); }
                }

                // Dispose client
                try { socketClient.Dispose(); }
                catch (Exception ex) { SafeOnError(ex, onError); }
            });

            try
            {
                foreach (string symbol in list)
                {
                    CallResult<UpdateSubscription> result = await socketClient.SpotApi.ExchangeData.SubscribeToTickerUpdatesAsync(
                        symbol,
                        data =>
                        {
                            try
                            {
                                IBinanceTick t = data.Data;

                                SymbolStats stats = new()
                                {
                                    Symbol = t.Symbol,
                                    Exchange = Exchange,
                                    FirstTradeId = t.FirstTradeId,
                                    OpenTime = t.OpenTime,
                                    OpenPrice = t.OpenPrice,
                                    CloseTime = t.CloseTime,
                                    PreviousDayClosePrice = t.PrevDayClosePrice,
                                    Volume = t.Volume,
                                    LowPrice = t.LowPrice,
                                    HighPrice = t.HighPrice,
                                    LastPrice = t.LastPrice,
                                    PriceChange = t.PriceChange,
                                    PriceChangePercent = t.PriceChangePercent,
                                    WeightedAveragePrice = t.WeightedAveragePrice,
                                    BestAskPrice = t.BestAskPrice,
                                    BestAskQuantity = t.BestAskQuantity,
                                    BestBidPrice = t.BestBidPrice,
                                    BestBidQuantity = t.BestBidQuantity,
                                    LastQuantity = t.LastQuantity,
                                    LastTradeId = t.LastTradeId,
                                    Period = t.CloseTime - t.OpenTime,
                                    QuoteVolume = t.QuoteVolume,
                                    TotalTrades = t.TotalTrades
                                };

                                SafeOnCallback(new StatisticsEventArgs { Statistics = [stats] }, onCallback, onError);
                            }
                            catch (Exception ex)
                            {
                                SafeOnError(ex, onError);
                            }
                        },
                        ct: cancellationToken
                    ).ConfigureAwait(false);

                    if (!result.Success)
                    {
                        throw new Exception($"SubscribeToSymbolStatisticsAsync({string.Join(",", list)}): {result.Error?.ToExchangeErrorMessage()}", result.Error?.Exception);
                    }

                    updateSubscriptions.Add(result.Data);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false);
                    throw new OperationCanceledException(cancellationToken);
                }

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(() =>
                    {
                        _ = Task.Run(async () =>
                        {
                            try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); }
                            catch (Exception ex) { SafeOnError(ex, onError); }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); } catch { /* ignore */ }
                SafeOnError(ex, onError);
                throw;
            }

            return asyncSubscriptionHandle;
        }

        public async Task<IAsyncDisposable> SubscribeToTradesAsync(string symbol, Action<TradeEventArgs> onCallback, Action<Exception> onError, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol);
            ArgumentNullException.ThrowIfNull(onCallback);
            ArgumentNullException.ThrowIfNull(onError);

            // IMPORTANT: do NOT 'using' this. The returned handle owns it.
            BinanceSocketClient socketClient = new();
            UpdateSubscription? updateSubscription = null;
            CancellationTokenRegistration cancellationTokenRegistration = default;

            AsyncSubscription asyncSubscriptionHandle = new(async () =>
            {
                // stop listening to token
                cancellationTokenRegistration.Dispose();

                // close subscription, then dispose client
                if (updateSubscription != null)
                {
                    try { await updateSubscription.CloseAsync().ConfigureAwait(false); }
                    catch (Exception ex) { SafeOnError(ex, onError); }
                }

                try { socketClient.Dispose(); }
                catch (Exception ex) { SafeOnError(ex, onError); }
            });

            try
            {
                CallResult<UpdateSubscription> result = await socketClient.SpotApi.ExchangeData
                    .SubscribeToAggregatedTradeUpdatesAsync(
                        symbol,
                        data =>
                        {
                            try
                            {
                                BinanceStreamAggregatedTrade t = data.Data;

                                Trade trade = new()
                                {
                                    Symbol = t.Symbol,
                                    Exchange = Exchange,
                                    Time = t.TradeTime,
                                    Id = t.Id,
                                    Price = t.Price,
                                    BaseQuantity = t.Quantity,
                                    QuoteQuantity = t.Quantity * t.Price
                                };

                                SafeOnCallback(new TradeEventArgs { Trades = [trade] }, onCallback, onError);
                            }
                            catch (Exception ex)
                            {
                                SafeOnError(ex, onError);
                            }
                        },
                        ct: cancellationToken
                    )
                    .ConfigureAwait(false);

                if (!result.Success)
                {
                    throw new Exception($"SubscribeToTradesAsync({symbol}) {result.Error?.ToExchangeErrorMessage()}", result.Error?.Exception);
                }

                updateSubscription = result.Data;

                if (cancellationToken.IsCancellationRequested)
                {
                    await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false);
                    throw new OperationCanceledException(cancellationToken);
                }

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(() =>
                    {
                        _ = Task.Run(async () =>
                        {
                            try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); }
                            catch (Exception ex) { SafeOnError(ex, onError); }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                // If subscribe fails, clean up what we created and rethrow
                try { await asyncSubscriptionHandle.DisposeAsync().ConfigureAwait(false); } catch { /* ignore */ }
                SafeOnError(ex, onError);
                throw;
            }

            return asyncSubscriptionHandle;
        }

        private static void SafeOnCallback<T>(T args, Action<T> onCallback, Action<Exception> onError)
        {
            try { onCallback(args); } catch (Exception ex) { SafeOnError(ex, onError); }
        }

        private static void SafeOnError(Exception ex, Action<Exception> onError)
        {
            try { onError(ex); } catch { /* swallow */ }
        }
    }
}
