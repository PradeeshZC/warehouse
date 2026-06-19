#nullable enable
namespace Warehouse.Models.DTOs
{
    public class InventoryStockDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string? ProductBarcode { get; set; }
        public string? CategoryName { get; set; }
        public int WarehouseEntityId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseLocation { get; set; }
        public int? BinId { get; set; }
        public string? BinCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal AvailableQuantity => Quantity - ReservedQuantity;
        public DateTime? LastUpdated { get; set; }

        /// <summary>Stock status derived from AvailableQuantity</summary>
        public string StockStatus => AvailableQuantity <= 0 ? "OutOfStock"
                                   : AvailableQuantity < 10 ? "LowStock"
                                   : "InStock";
    }
}
