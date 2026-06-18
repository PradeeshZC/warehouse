#nullable enable
using MediatR;

namespace Warehouse.CQRS.Queries
{
    public abstract class BaseQuery<TResponse> : IRequest<TResponse>
    {
    }
}
