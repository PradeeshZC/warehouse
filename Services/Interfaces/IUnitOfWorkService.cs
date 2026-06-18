#nullable enable
namespace Warehouse.Services.Interfaces
{
    public interface IUnitOfWorkService
    {
        Task<int> CommitAsync();
    }
}
