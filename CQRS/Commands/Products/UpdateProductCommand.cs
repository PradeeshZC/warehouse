#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Products
{
    public class UpdateProductCommand : BaseCommand<BaseResponse<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }
    }
}
