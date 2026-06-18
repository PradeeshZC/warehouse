#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Queries.Products
{
    public class GetProductByIdQuery : BaseQuery<BaseResponse<ProductDto>>
    {
        public int Id { get; set; }
    }
}
