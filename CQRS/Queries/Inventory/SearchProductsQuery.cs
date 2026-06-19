#nullable enable
using MediatR;
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Queries.Inventory
{
    /// <summary>Advanced product search query with warehouse/bin inventory details.</summary>
    public class SearchProductsQuery : IRequest<BaseResponse<PagedResult<ProductSearchResultDto>>>
    {
        public ProductSearchRequest Request { get; set; } = new();
    }
}
