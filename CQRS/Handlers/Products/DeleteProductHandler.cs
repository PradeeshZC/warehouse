#nullable enable
using MediatR;
using Warehouse.CQRS.Commands.Products;
using Warehouse.Models.DTOs;
using Warehouse.Repositories.Interfaces;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Products
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<Product> _repository;

        public DeleteProductHandler(IUnitOfWorkAsync unitOfWork, IRepository<Product> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return Result.Fail<bool>("Product not found");

            await _repository.DeleteAsync(product);
            await _unitOfWork.CompleteAsync();

            return Result.Ok(true);
        }
    }
}
