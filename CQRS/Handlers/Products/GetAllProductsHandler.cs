#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Queries.Products;
using Warehouse.Data;
using Warehouse.Models.DTOs;

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
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // ── Keyword search: Name, SKU, Barcode, Description ──
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.SKU.ToLower().Contains(term) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(term)) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            // ── Category filter ──
            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            query = query.OrderBy(p => p.Name);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductDto
                {
                    Id           = p.Id,
                    Name         = p.Name,
                    SKU          = p.SKU,
                    Barcode      = p.Barcode,
                    Description  = p.Description,
                    UnitPrice    = p.UnitPrice,
                    CategoryId   = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync(cancellationToken);

            return Result.Ok<IEnumerable<ProductDto>>(items);
        }
    }
}
