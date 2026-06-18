#nullable enable
namespace Warehouse.Models.DTOs
{
    public class InventoryStockDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int WarehouseEntityId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int? BinId { get; set; }
        public string? BinCode { get; set; }
        public decimal Quantity { get; set; }
    }
}
