#nullable enable
using MediatR;
using Warehouse.CQRS.Commands.Products;
using Warehouse.Models.DTOs;
using Warehouse.Repositories.Interfaces;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Products
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, BaseResponse<int>>
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<Product> _repository;

        public CreateProductHandler(IUnitOfWorkAsync unitOfWork, IRepository<Product> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<BaseResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var entity = new Product
            {
                Name = request.Name,
                SKU = request.SKU,
                Description = request.Description,
                UnitPrice = request.UnitPrice,
                CategoryId = request.CategoryId
            };

            await _repository.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return Result.Ok(entity.Id);
        }
    }
}
