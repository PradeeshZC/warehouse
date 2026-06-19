#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Queries.Products;
using Warehouse.Data;
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Handlers.Products
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, BaseResponse<ProductDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetProductByIdHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == request.Id && !p.IsDeleted)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                return Result.Fail<ProductDto>("Product not found");

            return Result.Ok(product);
        }
    }
}
