#nullable enable
namespace Warehouse.Models.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int BinCount { get; set; }
        public decimal TotalStockQuantity { get; set; }
    }
}
