namespace Cryoptix.Core.Models
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