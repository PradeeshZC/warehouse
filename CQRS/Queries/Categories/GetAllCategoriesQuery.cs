#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Queries.Categories
{
    public class GetAllCategoriesQuery : BaseQuery<BaseResponse<IEnumerable<CategoryDto>>>
    {
    }
}
