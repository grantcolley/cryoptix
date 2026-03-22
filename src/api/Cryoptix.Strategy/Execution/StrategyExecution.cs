using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Catalog;
using Cryoptix.Strategy.Runtime;
using Cryoptix.Strategy.Status;
using Cryoptix.Strategy.Strategies;

namespace Cryoptix.Strategy.Execution
{
    public class StrategyExecution(StrategyStateStore state, IStrategyCatalog catalog, IExchangeApiFactory exchangeApiFactory) : IStrategyExecution
    {
        private readonly StrategyStateStore _state = state;
        private readonly IStrategyCatalog _catalog = catalog;
        private readonly IExchangeApiFactory _exchangeApiFactory = exchangeApiFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private Task? _activeTask;
        private Runtime.Strategy? _activeStrategy;
        private CancellationTokenSource? _activeCancellationTokenSource;
        private AsyncAutoResetEvent? _activeStrategyUpdatedSignal;

        private int _disposeStarted;

        // Only call while holding _semaphoreSlim.
        private bool HasRunningActiveTask() => _activeTask != null && !_activeTask.IsCompleted;

        public async Task StartAsync(Runtime.Strategy strategy, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            await _semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                ThrowIfDisposed(); // double-check after acquiring semaphore

                if (HasRunningActiveTask())
                {
                    Runtime.Strategy? currentStrategy = Volatile.Read(ref _activeStrategy);

                    _state.Set(new StrategyStatus
                    {
                        StrategyState = StrategyState.Faulted,
                        StrategyType = strategy.StrategyType,
                        Strategy = strategy,
                        Message = $"Start rejected: strategy {currentStrategy?.StrategyType} {currentStrategy?.Name} already running. Stop first."
                    });

                    return;
                }

                if (!_catalog.TryCreate(strategy.StrategyType, out Func<IStrategyExecutable> strategyExecutionFactory))
                {
                    _state.Set(new StrategyStatus
                    {
                        StrategyState = StrategyState.Faulted,
                        StrategyType = strategy.StrategyType,
                        Strategy = strategy,
                        Message = $"Unknown strategy '{strategy.StrategyType}' {strategy.Name}"
                    });

                    return;
                }

                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Starting,
                    StrategyType = strategy.StrategyType,
                    Strategy = strategy
                });

                _activeCancellationTokenSource?.Dispose();
                _activeCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                CancellationToken activeCancellationToken = _activeCancellationTokenSource.Token;

                IStrategyExecutable strategyExecution = strategyExecutionFactory();

                Volatile.Write(ref _activeStrategy, strategy);
                
                AsyncAutoResetEvent activeStrategyUpdatedSignal = new();
                _activeStrategyUpdatedSignal = activeStrategyUpdatedSignal;

                StrategyRuntime strategyRuntime = new()
                { 
                    GetStrategy = () => Volatile.Read(ref _activeStrategy),
                    WaitForStrategyUpdateAsync = ct => activeStrategyUpdatedSignal.WaitAsync(ct),
                    ExchangeApi = _exchangeApiFactory.GetApi(strategy.Exchange)
                };

                Task runTask = RunStrategyAsync(strategy, strategyExecution, strategyRuntime, activeCancellationToken);

                _activeTask = runTask;

                _ = runTask.ContinueWith(
                    completedTask => OnRunCompletedAsync(completedTask),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default).Unwrap();
            }
            catch (Exception ex)
            {
                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Faulted,
                    StrategyType = strategy.StrategyType,
                    Strategy = strategy,
                    Message = ex.Message
                });
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task UpdateAsync(Runtime.Strategy strategy)
        {
            ThrowIfDisposed();

            AsyncAutoResetEvent? signalToSet = null;

            await _semaphoreSlim.WaitAsync();

            try
            {
                ThrowIfDisposed(); // double-check after acquiring semaphore

                // Only updates the currently running strategy config
                if (!HasRunningActiveTask())
                {
                    _state.Set(new StrategyStatus
                    {
                        StrategyState = StrategyState.Idle,
                    });

                    return;
                }

                Runtime.Strategy? currentStrategy = Volatile.Read(ref _activeStrategy);

                if (currentStrategy != null)
                {
                    if (!currentStrategy.CanUpdate(strategy, out string? message))
                    {
                        _state.Set(new StrategyStatus
                        {
                            StrategyState = StrategyState.Faulted,
                            StrategyType = currentStrategy.StrategyType,
                            Strategy = currentStrategy,
                            Message = message
                        });

                        return;
                    }
                }

                Volatile.Write(ref _activeStrategy, strategy);

                signalToSet = _activeStrategyUpdatedSignal;

                // Your strategies must read config via a shared reference if you want truly "live"
                // updates; otherwise use "restart on update" semantics.

                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Running,
                    StrategyType = strategy.StrategyType,
                    Strategy = strategy
                });
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            signalToSet?.Set();
        }

        public Task StopAsync()
        {
            return StopInternalAsync(throwIfDisposed: true);
        }

        private async Task StopInternalAsync(bool throwIfDisposed)
        {
            if (throwIfDisposed)
            {
                ThrowIfDisposed();
            }

            Task? taskToAwait = null;
            CancellationTokenSource? cancellationTokenSourceToCancel = null;
            
            await _semaphoreSlim.WaitAsync();

            try
            {
                // double-check after acquiring semaphore
                if (throwIfDisposed)
                {
                    ThrowIfDisposed();
                }

                if (!HasRunningActiveTask())
                {
                    CleanupActiveExecution();

                    _state.Set(new StrategyStatus
                    {
                        StrategyState = StrategyState.Idle,
                    });

                    return;
                }

                taskToAwait = _activeTask;
                cancellationTokenSourceToCancel = _activeCancellationTokenSource;
                Runtime.Strategy? strategyToReport = Volatile.Read(ref _activeStrategy);

                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Stopping,
                    StrategyType = strategyToReport?.StrategyType ?? StrategyType.None,
                    Strategy = strategyToReport
                });
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            cancellationTokenSourceToCancel?.Cancel();

            try
            {
                await taskToAwait!;
            }
            catch (OperationCanceledException)
            {
                // Normal
            }
            catch
            {
                // The background task already reports fault state.
                // Stop still proceeds with cleanup.
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                // Only clean up if this is still the same active task.
                if (ReferenceEquals(_activeTask, taskToAwait))
                {
                    CleanupActiveExecution();

                    _state.Set(new StrategyStatus
                    {
                        StrategyState = StrategyState.Idle
                    });
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
                return;

            try
            {
                await StopInternalAsync(throwIfDisposed: false);
            }
            catch
            {
                // Never throw from DisposeAsync
            }

            _activeCancellationTokenSource?.Dispose();
            _activeCancellationTokenSource = null;
            _semaphoreSlim.Dispose();

            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeStarted) != 0, this);
        }

        private void CleanupActiveExecution()
        {
            _activeTask = null;
            _activeCancellationTokenSource?.Dispose();
            _activeCancellationTokenSource = null;
            Volatile.Write(ref _activeStrategy, null);
            _activeStrategyUpdatedSignal = null;
        }

        private async Task RunStrategyAsync(
            Runtime.Strategy strategy,
            IStrategyExecutable strategyExecutable,
            StrategyRuntime strategyRuntime,
            CancellationToken cancellationToken)
        {
            try
            {
                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Running,
                    StrategyType = strategy.StrategyType,
                    Strategy = strategy
                });

                await strategyExecutable.ExecuteAsync(strategyRuntime, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Faulted,
                    StrategyType = strategy.StrategyType,
                    Strategy = strategy,
                    Message = ex.Message
                });

                throw;
            }
        }

        private async Task OnRunCompletedAsync(Task completedTask)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                if (!ReferenceEquals(_activeTask, completedTask))
                {
                    return;
                }

                CleanupActiveExecution();

                if (completedTask.IsFaulted)
                {
                    // Fault state already set in RunStrategyAsync
                    return;
                }

                _state.Set(new StrategyStatus
                {
                    StrategyState = StrategyState.Idle
                });
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
