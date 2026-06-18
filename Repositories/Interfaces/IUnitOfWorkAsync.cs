#nullable enable
namespace Warehouse.Repositories.Interfaces
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
