#nullable enable
using MediatR;
using Warehouse.CQRS.Commands.Categories;
using Warehouse.Models.DTOs;
using Warehouse.Repositories.Interfaces;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Categories
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<Category> _repository;

        public UpdateCategoryHandler(IUnitOfWorkAsync unitOfWork, IRepository<Category> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.Id);
            if (category == null)
                return Result.Fail<bool>("Category not found");

            category.Name = request.Name;
            category.Description = request.Description;

            await _repository.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();

            return Result.Ok(true);
        }
    }
}
