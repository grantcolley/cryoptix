using Cryoptix.Market.Data;

namespace Cryoptix.Strategy.Snapshot
{
    public sealed class AccountRealtimeState
    {
        private readonly Lock _gate = new();

        private Account? _account;

        private Dictionary<string, Balance> _balancesByAsset = new(StringComparer.Ordinal);

        private static string NormalizeAsset(string asset) => asset.Trim().ToUpperInvariant();

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

        public bool TryGet(out Account? account)
        {
            lock (_gate)
            {
                account = _account == null ? null : CloneAccount(_account);
                return account != null;
            }
        }

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

        public bool HasFreeBalance(string asset, decimal minimumFreeBalance)
        {
            return GetFreeBalance(asset) >= minimumFreeBalance;
        }

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
