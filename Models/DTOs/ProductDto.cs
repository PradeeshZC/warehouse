#nullable enable
namespace Warehouse.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
