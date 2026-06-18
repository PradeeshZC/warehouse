#nullable enable
namespace Warehouse.CQRS.Handlers
{
    public interface ICommandHandler<TCommand>
        where TCommand : class
    {
        Task HandleAsync(TCommand command);
    }
}
