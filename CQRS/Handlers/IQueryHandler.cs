#nullable enable
namespace Warehouse.CQRS.Handlers
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : class
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
