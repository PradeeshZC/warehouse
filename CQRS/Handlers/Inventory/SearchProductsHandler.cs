#nullable enable
using MediatR;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Queries.Inventory;
using Warehouse.Data;
using Warehouse.Models.DTOs;

namespace Warehouse.CQRS.Handlers.Inventory
{
    /// <summary>
    /// Advanced product search handler.
    ///
    /// Design decisions:
    ///   • Uses IQueryable + AsNoTracking() — no full entity graph loaded
    ///   • Left join to InventoryStocks so products with no stock still appear
    ///   • Select() projection — only needed columns returned from SQL
    ///   • Supports: keyword (Name/SKU/Barcode/Category), CategoryId, WarehouseId, BinId, StockStatus
    ///   • Supports: SortBy (Name/SKU/Category/Quantity/Warehouse) + SortDirection (asc/desc)
    ///   • Paginated with Skip/Take — never loads full table
    /// </summary>
    public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, BaseResponse<PagedResult<ProductSearchResultDto>>>
    {
        private readonly ApplicationDbContext _context;

        public SearchProductsHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<PagedResult<ProductSearchResultDto>>> Handle(
            SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            // ── Base query: Products LEFT JOIN InventoryStocks (grouped by product) ──
            // We use a sub-select approach: group stock by ProductId to get aggregated totals
            var stockSummary = _context.InventoryStocks
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { s.ProductId, s.WarehouseEntityId, s.BinId })
                .Select(g => new
                {
                    ProductId         = g.Key.ProductId,
                    WarehouseEntityId = g.Key.WarehouseEntityId,
                    BinId             = g.Key.BinId,
                    TotalQty          = g.Sum(x => x.Quantity),
                    TotalReserved     = g.Sum(x => x.ReservedQuantity),
                    StockId           = g.Min(x => x.Id)
                });

            // Main product query with left join to stock summary
            var query = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .GroupJoin(
                    stockSummary,
                    p => p.Id,
                    s => s.ProductId,
                    (p, stocks) => new { p, stocks })
                .SelectMany(
                    x => x.stocks.DefaultIfEmpty(),
                    (x, stock) => new
                    {
                        x.p.Id,
                        x.p.Name,
                        x.p.SKU,
                        x.p.Barcode,
                        x.p.UnitPrice,
                        x.p.CategoryId,
                        CategoryName      = x.p.Category.Name,
                        StockId           = (int?)stock!.StockId,
                        WarehouseEntityId = (int?)stock.WarehouseEntityId,
                        WarehouseName     = stock != null ? x.p.InventoryStocks
                                               .Where(s => s.Id == stock.StockId)
                                               .Select(s => s.WarehouseEntity.Name)
                                               .FirstOrDefault() : null,
                        WarehouseLocation = stock != null ? x.p.InventoryStocks
                                               .Where(s => s.Id == stock.StockId)
                                               .Select(s => s.WarehouseEntity.Location)
                                               .FirstOrDefault() : null,
                        BinId   = (int?)stock!.BinId,
                        BinCode = stock != null ? x.p.InventoryStocks
                                     .Where(s => s.Id == stock.StockId)
                                     .Select(s => s.Bin != null ? s.Bin.Code : null)
                                     .FirstOrDefault() : null,
                        TotalQty      = stock != null ? stock.TotalQty : 0m,
                        TotalReserved = stock != null ? stock.TotalReserved : 0m
                    });

            // ── Keyword search ──
            if (!string.IsNullOrWhiteSpace(r.SearchText))
            {
                var term = r.SearchText.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(term) ||
                    x.SKU.ToLower().Contains(term) ||
                    (x.Barcode != null && x.Barcode.ToLower().Contains(term)) ||
                    (x.CategoryName != null && x.CategoryName.ToLower().Contains(term)));
            }

            // ── Filters ──
            if (r.CategoryId.HasValue && r.CategoryId > 0)
                query = query.Where(x => x.CategoryId == r.CategoryId.Value);

            if (r.WarehouseId.HasValue && r.WarehouseId > 0)
                query = query.Where(x => x.WarehouseEntityId == r.WarehouseId.Value);

            if (r.BinId.HasValue && r.BinId > 0)
                query = query.Where(x => x.BinId == r.BinId.Value);

            if (!string.IsNullOrWhiteSpace(r.StockStatus))
            {
                query = r.StockStatus switch
                {
                    "InStock"    => query.Where(x => (x.TotalQty - x.TotalReserved) >= 10),
                    "LowStock"   => query.Where(x => (x.TotalQty - x.TotalReserved) > 0 && (x.TotalQty - x.TotalReserved) < 10),
                    "OutOfStock" => query.Where(x => (x.TotalQty - x.TotalReserved) <= 0),
                    _            => query
                };
            }

            // ── Count before pagination ──
            var totalCount = await query.CountAsync(cancellationToken);

            // ── Sorting ──
            query = (r.SortBy?.ToLower(), r.SortDirection?.ToLower()) switch
            {
                ("sku",       "desc") => query.OrderByDescending(x => x.SKU),
                ("sku",          _  ) => query.OrderBy(x => x.SKU),
                ("category",  "desc") => query.OrderByDescending(x => x.CategoryName),
                ("category",     _  ) => query.OrderBy(x => x.CategoryName),
                ("quantity",  "desc") => query.OrderByDescending(x => x.TotalQty),
                ("quantity",     _  ) => query.OrderBy(x => x.TotalQty),
                ("warehouse", "desc") => query.OrderByDescending(x => x.WarehouseName),
                ("warehouse",    _  ) => query.OrderBy(x => x.WarehouseName),
                (_,           "desc") => query.OrderByDescending(x => x.Name),
                _                    => query.OrderBy(x => x.Name)
            };

            // ── Pagination ──
            var pageSize   = Math.Clamp(r.PageSize, 1, 100);
            var pageNumber = Math.Max(r.PageNumber, 1);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProductSearchResultDto
                {
                    ProductId         = x.Id,
                    Name              = x.Name,
                    SKU               = x.SKU,
                    Barcode           = x.Barcode,
                    CategoryName      = x.CategoryName,
                    UnitPrice         = x.UnitPrice,
                    StockId           = x.StockId,
                    WarehouseEntityId = x.WarehouseEntityId,
                    WarehouseName     = x.WarehouseName,
                    WarehouseLocation = x.WarehouseLocation,
                    BinId             = x.BinId,
                    BinCode           = x.BinCode,
                    TotalQuantity     = x.TotalQty,
                    TotalReserved     = x.TotalReserved,
                    LocationCount     = 1
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ProductSearchResultDto>
            {
                Items      = items,
                TotalCount = totalCount,
                Page       = pageNumber,
                PageSize   = pageSize
            };

            return Result.Ok(result);
        }
    }
}
