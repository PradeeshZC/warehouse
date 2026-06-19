#nullable enable
namespace Warehouse.Models.DTOs
{
    /// <summary>
    /// Optimised projection DTO for advanced product search results.
    /// Includes inventory location details without returning full entity graphs.
    /// </summary>
    public class ProductSearchResultDto
    {
        // Product Information
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? CategoryName { get; set; }
        public decimal UnitPrice { get; set; }

        // Inventory Information (from first/primary stock record)
        public int? StockId { get; set; }
        public int? WarehouseEntityId { get; set; }
        public string? WarehouseName { get; set; }
        public string? WarehouseLocation { get; set; }
        public int? BinId { get; set; }
        public string? BinCode { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalReserved { get; set; }
        public decimal AvailableQuantity => TotalQuantity - TotalReserved;

        /// <summary>Number of different warehouse locations this product exists in</summary>
        public int LocationCount { get; set; }

        /// <summary>Derived stock status</summary>
        public string StockStatus => AvailableQuantity <= 0 ? "OutOfStock"
                                   : AvailableQuantity < 10 ? "LowStock"
                                   : "InStock";
    }
}
