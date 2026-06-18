using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Queries.Categories;
using Warehouse.Models.DTOs;
using Warehouse.Data;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Categories
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, BaseResponse<IEnumerable<CategoryDto>>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllCategoriesHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<IEnumerable<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .ToListAsync(cancellationToken);

            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products?.Count ?? 0
            });

            return Result.Ok(dtos);
        }
    }
}
