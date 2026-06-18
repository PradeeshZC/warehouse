#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Categories
{
    public class UpdateCategoryCommand : BaseCommand<BaseResponse<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
