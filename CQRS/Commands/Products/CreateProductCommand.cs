#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Products
{
    public class CreateProductCommand : BaseCommand<BaseResponse<int>>
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }

        // ── Optional: create initial inventory on product creation ──
        public decimal? InitialQuantity { get; set; }
        public int? WarehouseEntityId { get; set; }
        public int? BinId { get; set; }
    }
}
