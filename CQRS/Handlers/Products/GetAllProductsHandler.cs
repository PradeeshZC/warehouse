#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Queries.Products;
using Warehouse.Models.DTOs;
using Warehouse.Data;
using Warehouse.Models.Entities;

namespace Warehouse.CQRS.Handlers.Products
{
    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, BaseResponse<IEnumerable<ProductDto>>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllProductsHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term)
                    || p.SKU.ToLower().Contains(term)
                    || (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Description = p.Description,
                    UnitPrice = p.UnitPrice,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync(cancellationToken);

            return Result.Ok<IEnumerable<ProductDto>>(items);
        }
    }
}
