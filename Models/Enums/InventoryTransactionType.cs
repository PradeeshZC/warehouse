#nullable enable
namespace Warehouse.Models.Enums
{
    public enum InventoryTransactionType
    {
        StockIn = 0,
        StockOut = 1,
        Transfer = 2,
        Reservation = 3,
        ReleaseReservation = 4,
        Adjustment = 5,
        PurchaseReceive = 6,
        OrderDispatch = 7
    }
}
