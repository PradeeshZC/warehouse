#nullable enable
namespace Warehouse.Models.DTOs
{
    public class BinDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int WarehouseEntityId { get; set; }
        public string? WarehouseName { get; set; }
        public int StockCount { get; set; }
        public decimal TotalQuantity { get; set; }
    }
}
