#nullable enable
namespace Warehouse.Models.DTOs
{
    /// <summary>Advanced product search request supporting full-text search, filters, sorting and pagination.</summary>
    public class ProductSearchRequest
    {
        public string? SearchText { get; set; }
        public int? CategoryId { get; set; }
        public int? WarehouseId { get; set; }
        public int? BinId { get; set; }

        /// <summary>Filter by stock status: All | InStock | LowStock | OutOfStock</summary>
        public string? StockStatus { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        /// <summary>Column to sort by: Name | SKU | Category | Quantity | Warehouse</summary>
        public string? SortBy { get; set; } = "Name";

        /// <summary>asc or desc</summary>
        public string? SortDirection { get; set; } = "asc";
    }
}
