#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Queries.Products
{
    public class GetAllProductsQuery : BaseQuery<BaseResponse<IEnumerable<ProductDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SearchTerm { get; set; }
    }
}
