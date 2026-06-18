#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Categories
{
    public class DeleteCategoryCommand : BaseCommand<BaseResponse<bool>>
    {
        public int Id { get; set; }
    }
}
