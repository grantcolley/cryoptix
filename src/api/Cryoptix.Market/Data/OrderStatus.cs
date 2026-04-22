namespace Cryoptix.Market.Data
{
    public enum OrderStatus
    {
        PendingNew,
        New,
        PartiallyFilled,
        Filled,
        Canceled,
        PendingCancel,
        Rejected,
        Expired,
        Unknown
    }
}