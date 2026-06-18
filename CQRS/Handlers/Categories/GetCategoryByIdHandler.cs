using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Queries.Categories;
using Warehouse.Models.DTOs;
using Warehouse.Data;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Categories
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, BaseResponse<CategoryDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetCategoryByIdHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
                return Result.Fail<CategoryDto>("Category not found");

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products?.Count ?? 0
            };

            return Result.Ok(dto);
        }
    }
}
