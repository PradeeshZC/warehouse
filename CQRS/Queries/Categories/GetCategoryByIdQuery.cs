#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Queries.Categories
{
    public class GetCategoryByIdQuery : BaseQuery<BaseResponse<CategoryDto>>
    {
        public int Id { get; set; }
    }
}
