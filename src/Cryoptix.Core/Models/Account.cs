using Cryoptix.Core.Enums;

namespace Cryoptix.Core.Models
{
    public class Account
    {
        public Account()
        {
            Balances = [];
        }

        public string? Name { get; set; }
        public Exchange Exchange { get; set; }
        public DateTime Time { get; set; }
        public decimal BuyerFee { get; set; }
        public decimal SellerFee { get; set; }
        public List<Balance> Balances { get; private set; }
    }
}
