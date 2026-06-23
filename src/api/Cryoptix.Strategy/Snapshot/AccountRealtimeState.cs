using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Snapshot
{
    /// <summary>
    /// Represents the account realtime state.
    /// </summary>
    public sealed class AccountRealtimeState
    {
        private readonly Lock _gate = new();

        private Account? _account;

        private Dictionary<string, Balance> _balancesByAsset = new(StringComparer.Ordinal);

        private static string NormalizeAsset(string asset) => asset.Trim().ToUpperInvariant();

        /// <summary>
        /// Executes the update operation.
        /// </summary>
        /// <param name="account">The account value.</param>
        public void Update(Account account)
        {
            ArgumentNullException.ThrowIfNull(account);

            Account clonedAccount = CloneAccount(account);

            Dictionary<string, Balance> balances = new(StringComparer.Ordinal);

            foreach (Balance balance in clonedAccount.Balances)
            {
                if (string.IsNullOrWhiteSpace(balance.Asset))
                    continue;

                string key = NormalizeAsset(balance.Asset);

                balances[key] = balance;
            }

            lock (_gate)
            {
                _account = clonedAccount;
                _balancesByAsset = balances;
            }
        }

        /// <summary>
        /// Executes the try get operation.
        /// </summary>
        /// <param name="account">The account value.</param>
        /// <returns>The try get result.</returns>
        public bool TryGet(out Account? account)
        {
            lock (_gate)
            {
                account = _account == null ? null : CloneAccount(_account);
                return account != null;
            }
        }

        /// <summary>
        /// Executes the get free balance operation.
        /// </summary>
        /// <param name="asset">The asset value.</param>
        /// <returns>The get free balance result.</returns>
        public decimal GetFreeBalance(string asset)
        {
            ArgumentNullException.ThrowIfNull(asset);

            string key = NormalizeAsset(asset);

            lock (_gate)
            {
                if (_balancesByAsset.TryGetValue(key, out Balance? balance))
                {
                    return balance.Free;
                }

                return 0m;
            }
        }

        /// <summary>
        /// Executes the has free balance operation.
        /// </summary>
        /// <param name="asset">The asset value.</param>
        /// <param name="minimumFreeBalance">The minimum free balance value.</param>
        /// <returns>The has free balance result.</returns>
        public bool HasFreeBalance(string asset, decimal minimumFreeBalance)
        {
            return GetFreeBalance(asset) >= minimumFreeBalance;
        }

        /// <summary>
        /// Executes the get locked balance operation.
        /// </summary>
        /// <param name="asset">The asset value.</param>
        /// <returns>The get locked balance result.</returns>
        public decimal GetLockedBalance(string asset)
        {
            ArgumentNullException.ThrowIfNull(asset);

            string key = NormalizeAsset(asset);

            lock (_gate)
            {
                if (_balancesByAsset.TryGetValue(key, out Balance? balance))
                {
                    return balance.Locked;
                }

                return 0m;
            }
        }

        /// <summary>
        /// Executes the get total balance operation.
        /// </summary>
        /// <param name="asset">The asset value.</param>
        /// <returns>The get total balance result.</returns>
        public decimal GetTotalBalance(string asset)
        {
            ArgumentNullException.ThrowIfNull(asset);

            string key = NormalizeAsset(asset);

            lock (_gate)
            {
                if (_balancesByAsset.TryGetValue(key, out Balance? balance))
                {
                    return balance.Total;
                }

                return 0m;
            }
        }

        /// <summary>
        /// Executes the get portfolio value operation.
        /// </summary>
        /// <returns>The get portfolio value result.</returns>
        public decimal GetPortfolioValue()
        {
            // Subscribe to symbol statistics to compute total portfolio value.
            throw new NotImplementedException();
        }

        private static Account CloneAccount(Account source)
        {
            Account clone = new()
            {
                Name = source.Name,
                Exchange = source.Exchange,
                Time = source.Time,
                BuyerFee = source.BuyerFee,
                SellerFee = source.SellerFee
            };

            foreach (Balance balance in source.Balances)
            {
                clone.Balances.Add(new Balance
                {
                    Asset = balance.Asset,
                    Free = balance.Free,
                    Locked = balance.Locked
                });
            }

            return clone;
        }
    }
}
