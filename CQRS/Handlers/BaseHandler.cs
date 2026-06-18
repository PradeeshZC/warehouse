#nullable enable
using MediatR;
using Warehouse.Repositories.Interfaces;

namespace Warehouse.CQRS.Handlers
{
    public abstract class BaseHandler
    {
        protected readonly IUnitOfWorkAsync _unitOfWork;

        protected BaseHandler(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
