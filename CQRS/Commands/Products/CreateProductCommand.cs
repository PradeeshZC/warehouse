#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Products
{
    public class CreateProductCommand : BaseCommand<BaseResponse<int>>
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }
    }
}
