#nullable enable
using MediatR;
using Warehouse.CQRS.Commands.Products;
using Warehouse.Models.DTOs;
using Warehouse.Repositories.Interfaces;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Products
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<Product> _repository;

        public UpdateProductHandler(IUnitOfWorkAsync unitOfWork, IRepository<Product> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return Result.Fail<bool>("Product not found");

            product.Name = request.Name;
            product.SKU = request.SKU;
            product.Description = request.Description;
            product.UnitPrice = request.UnitPrice;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(product);
            await _unitOfWork.CompleteAsync();

            return Result.Ok(true);
        }
    }
}
