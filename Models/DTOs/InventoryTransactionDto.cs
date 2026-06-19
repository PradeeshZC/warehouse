#nullable enable
using Warehouse.Models.Enums;

namespace Warehouse.Models.DTOs
{
    public class InventoryTransactionDto
    {
        public int InventoryTransactionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSKU { get; set; }
        public int WarehouseEntityId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int? BinId { get; set; }
        public string? BinCode { get; set; }
        public InventoryTransactionType TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public decimal PreviousQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public decimal QuantityDifference => NewQuantity - PreviousQuantity;
        public string? ReferenceNumber { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
