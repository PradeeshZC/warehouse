#nullable enable
namespace Warehouse.Modules.Warehouses.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
