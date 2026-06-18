#nullable enable
using MediatR;
using Warehouse.CQRS.Commands.Categories;
using Warehouse.Models.DTOs;
using Warehouse.Models.Entities;
using Warehouse.Repositories.Interfaces;

namespace Warehouse.CQRS.Handlers.Categories
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, BaseResponse<int>>
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<Category> _repository;

        public CreateCategoryHandler(IUnitOfWorkAsync unitOfWork, IRepository<Category> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<BaseResponse<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category { Name = request.Name, Description = request.Description };
            await _repository.AddAsync(category);
            await _unitOfWork.CompleteAsync();
            return Result.Ok(category.Id);
        }
    }
}
