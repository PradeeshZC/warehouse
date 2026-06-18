#nullable enable
namespace Warehouse.Models.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Packed = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5
    }
}
