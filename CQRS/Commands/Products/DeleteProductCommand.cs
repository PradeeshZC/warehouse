#nullable enable
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Commands.Products
{
    public class DeleteProductCommand : BaseCommand<BaseResponse<bool>>
    {
        public int Id { get; set; }
    }
}
