#nullable enable
using MediatR;

namespace Warehouse.CQRS.Commands
{
    // Marker base command — response type parameterized
    public abstract class BaseCommand<TResponse> : IRequest<TResponse>
    {
    }
}
