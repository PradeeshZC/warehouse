#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Categories
{
    public class CreateCategoryCommand : BaseCommand<BaseResponse<int>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
