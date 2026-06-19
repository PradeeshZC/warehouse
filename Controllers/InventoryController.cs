#nullable enable
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Commands.Inventory;
using Warehouse.CQRS.Queries.Inventory;
using Warehouse.Data;
using Warehouse.Models.DTOs;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;
using Warehouse.Models.ViewModels;

namespace Warehouse.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public InventoryController(ApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /Inventory
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            int? warehouseId, int? binId, string? search,
            int page = 1, int pageSize = 20)
        {
            var query = _context.InventoryStocks
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            if (warehouseId.HasValue && warehouseId.Value > 0)
                query = query.Where(s => s.WarehouseEntityId == warehouseId.Value);

            if (binId.HasValue && binId.Value > 0)
                query = query.Where(s => s.BinId == binId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s =>
                    s.Product.Name.ToLower().Contains(term) ||
                    s.Product.SKU.ToLower().Contains(term) ||
                    (s.Product.Barcode != null && s.Product.Barcode.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var stocks = await query
                .OrderBy(s => s.Product.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new InventoryStockDto
                {
                    Id                = s.Id,
                    ProductId         = s.ProductId,
                    ProductName       = s.Product.Name,
                    ProductSKU        = s.Product.SKU,
                    ProductBarcode    = s.Product.Barcode,
                    CategoryName      = s.Product.Category.Name,
                    WarehouseEntityId = s.WarehouseEntityId,
                    WarehouseName     = s.WarehouseEntity.Name,
                    WarehouseLocation = s.WarehouseEntity.Location,
                    BinId             = s.BinId,
                    BinCode           = s.Bin != null ? s.Bin.Code : null,
                    Quantity          = s.Quantity,
                    ReservedQuantity  = s.ReservedQuantity,
                    LastUpdated       = s.UpdatedAt ?? s.CreatedAt
                })
                .ToListAsync();

            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.Name)
                .ToListAsync();

            ViewBag.WarehouseId  = warehouseId;
            ViewBag.BinId        = binId;
            ViewBag.Search       = search;
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = totalPages;
            ViewBag.TotalCount   = totalCount;
            ViewBag.Warehouses   = new SelectList(warehouses, "Id", "Name", warehouseId);

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                var bins = await _context.Bins
                    .AsNoTracking()
                    .Where(b => b.WarehouseEntityId == warehouseId.Value && !b.IsDeleted)
                    .OrderBy(b => b.Code)
                    .ToListAsync();
                ViewBag.Bins = new SelectList(bins, "Id", "Code", binId);
            }
            else
            {
                ViewBag.Bins = new SelectList(Enumerable.Empty<Bin>(), "Id", "Code");
            }

            return View(stocks);
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /Inventory/Transactions
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Transactions(
            string? search, int? typeFilter, int page = 1, int pageSize = 25)
        {
            var query = _context.InventoryTransactions
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(t =>
                    (t.Product != null && t.Product.Name.ToLower().Contains(term)) ||
                    (t.Product != null && t.Product.SKU.ToLower().Contains(term)) ||
                    (t.WarehouseEntity != null && t.WarehouseEntity.Name.ToLower().Contains(term)));
            }

            if (typeFilter.HasValue)
                query = query.Where(t => (int)t.TransactionType == typeFilter.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new InventoryTransactionDto
                {
                    InventoryTransactionId = t.InventoryTransactionId,
                    ProductId              = t.ProductId,
                    ProductName            = t.Product != null ? t.Product.Name : string.Empty,
                    ProductSKU             = t.Product != null ? t.Product.SKU : null,
                    WarehouseEntityId      = t.WarehouseEntityId,
                    WarehouseName          = t.WarehouseEntity != null ? t.WarehouseEntity.Name : string.Empty,
                    BinId                  = t.BinId,
                    BinCode                = t.Bin != null ? t.Bin.Code : null,
                    TransactionType        = t.TransactionType,
                    Quantity               = t.Quantity,
                    PreviousQuantity       = t.PreviousQuantity,
                    NewQuantity            = t.NewQuantity,
                    ReferenceNumber        = t.ReferenceNumber,
                    Reason                 = t.Reason,
                    Notes                  = t.Notes,
                    CreatedByUserId        = t.CreatedByUserId,
                    CreatedAt              = t.CreatedAt
                })
                .ToListAsync();

            ViewBag.Search      = search;
            ViewBag.TypeFilter  = typeFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = totalPages;
            ViewBag.TotalCount  = totalCount;
            ViewBag.TransactionTypes = Enum.GetValues<InventoryTransactionType>()
                .Select(t => new SelectListItem(t.ToString(), ((int)t).ToString()))
                .ToList();

            return View(transactions);
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /Inventory/AdjustStock/{stockId}  — dedicated adjust flow
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AdjustStock(int stockId)
        {
            var stock = await _context.InventoryStocks
                .AsNoTracking()
                .Where(s => s.Id == stockId && !s.IsDeleted)
                .Select(s => new AdjustStockViewModel
                {
                    StockId           = s.Id,
                    ProductId         = s.ProductId,
                    ProductName       = s.Product.Name,
                    ProductSKU        = s.Product.SKU,
                    ProductBarcode    = s.Product.Barcode,
                    CategoryName      = s.Product.Category.Name,
                    WarehouseEntityId = s.WarehouseEntityId,
                    WarehouseName     = s.WarehouseEntity.Name,
                    WarehouseLocation = s.WarehouseEntity.Location,
                    BinId             = s.BinId,
                    BinCode           = s.Bin != null ? s.Bin.Code : null,
                    CurrentQuantity   = s.Quantity,
                    ReservedQuantity  = s.ReservedQuantity,
                    LastUpdated       = s.UpdatedAt ?? s.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (stock == null)
            {
                TempData["ErrorMessage"] = "Inventory stock record not found.";
                return RedirectToAction(nameof(Index));
            }

            // Last stock movement date
            var lastTx = await _context.InventoryTransactions
                .AsNoTracking()
                .Where(t => t.ProductId == stock.ProductId &&
                            t.WarehouseEntityId == stock.WarehouseEntityId &&
                            t.BinId == stock.BinId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => (DateTime?)t.CreatedAt)
                .FirstOrDefaultAsync();

            stock.LastStockMovement = lastTx;

            return View(stock);
        }

        // ─────────────────────────────────────────────────────────────
        // POST: /Inventory/AdjustStock — dedicated CQRS-based adjust
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(AdjustStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload read-only details before returning the view
                var refreshed = await _context.InventoryStocks
                    .AsNoTracking()
                    .Where(s => s.Id == model.StockId && !s.IsDeleted)
                    .Select(s => new
                    {
                        ProductName       = s.Product.Name,
                        ProductSKU        = s.Product.SKU,
                        ProductBarcode    = s.Product.Barcode,
                        CategoryName      = s.Product.Category.Name,
                        WarehouseName     = s.WarehouseEntity.Name,
                        WarehouseLocation = s.WarehouseEntity.Location,
                        BinCode           = s.Bin != null ? s.Bin.Code : null,
                        s.Quantity,
                        s.ReservedQuantity,
                        s.UpdatedAt,
                        s.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (refreshed != null)
                {
                    model.ProductName       = refreshed.ProductName;
                    model.ProductSKU        = refreshed.ProductSKU;
                    model.ProductBarcode    = refreshed.ProductBarcode;
                    model.CategoryName      = refreshed.CategoryName;
                    model.WarehouseName     = refreshed.WarehouseName;
                    model.WarehouseLocation = refreshed.WarehouseLocation;
                    model.BinCode           = refreshed.BinCode;
                    model.CurrentQuantity   = refreshed.Quantity;
                    model.ReservedQuantity  = refreshed.ReservedQuantity;
                    model.LastUpdated       = refreshed.UpdatedAt ?? refreshed.CreatedAt;
                }
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var command = new AdjustStockCommand
            {
                StockId        = model.StockId,
                AdjustmentType = model.AdjustmentType,
                QuantityChange = model.QuantityChange,
                Reason         = model.Reason,
                Notes          = model.Notes,
                UserId         = userId
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Stock adjusted successfully. {model.AdjustmentType} by {model.QuantityChange:N2} units. Reason: {model.Reason}";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Adjustment failed.");
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /Inventory/Adjust — existing general-purpose adjust / transfer
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Adjust()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // POST: /Inventory/Adjust — existing general-purpose adjust / transfer
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(
            int productId,
            int warehouseEntityId,
            int? binId,
            InventoryTransactionType transactionType,
            decimal quantity,
            string? notes,
            int? destinationWarehouseEntityId,
            int? destinationBinId)
        {
            if (productId <= 0 || warehouseEntityId <= 0 || quantity <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please provide valid Product, Warehouse, and a positive Quantity.");
                await PopulateDropdownsAsync();
                return View();
            }

            int? actualBinId = (binId.HasValue && binId.Value > 0) ? binId.Value : null;

            if (transactionType == InventoryTransactionType.Transfer)
            {
                if (!destinationWarehouseEntityId.HasValue || destinationWarehouseEntityId.Value <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Please specify a destination warehouse for the transfer.");
                    await PopulateDropdownsAsync();
                    return View();
                }

                int? actualDestBinId = (destinationBinId.HasValue && destinationBinId.Value > 0) ? destinationBinId.Value : null;

                if (warehouseEntityId == destinationWarehouseEntityId.Value && actualBinId == actualDestBinId)
                {
                    ModelState.AddModelError(string.Empty, "Source and destination locations must be different.");
                    await PopulateDropdownsAsync();
                    return View();
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (transactionType == InventoryTransactionType.Transfer)
                {
                    int destWarehouseId = destinationWarehouseEntityId!.Value;
                    int? actualDestBinId = (destinationBinId.HasValue && destinationBinId.Value > 0) ? destinationBinId.Value : null;

                    var sourceStock = await _context.InventoryStocks
                        .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseEntityId == warehouseEntityId && s.BinId == actualBinId);

                    decimal sourcePrevQty = sourceStock?.Quantity ?? 0m;
                    if (sourcePrevQty < quantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Insufficient stock at source location. Current quantity is {sourcePrevQty}.");
                        await PopulateDropdownsAsync();
                        return View();
                    }

                    sourceStock!.Quantity -= quantity;

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = productId, WarehouseEntityId = warehouseEntityId,
                        TransactionType = InventoryTransactionType.Transfer, Quantity = quantity,
                        PreviousQuantity = sourcePrevQty, NewQuantity = sourcePrevQty - quantity,
                        Notes = $"Transfer Out to Warehouse ID {destWarehouseId}. {notes}",
                        CreatedByUserId = userId
                    });

                    var destStock = await _context.InventoryStocks
                        .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseEntityId == destWarehouseId && s.BinId == actualDestBinId);

                    decimal destPrevQty = destStock?.Quantity ?? 0m;
                    if (destStock == null)
                    {
                        destStock = new InventoryStock
                        {
                            ProductId = productId, WarehouseEntityId = destinationWarehouseEntityId.Value,
                            BinId = actualDestBinId, Quantity = quantity
                        };
                        _context.InventoryStocks.Add(destStock);
                    }
                    else
                    {
                        destStock.Quantity += quantity;
                    }

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = productId, WarehouseEntityId = destinationWarehouseEntityId.Value,
                        TransactionType = InventoryTransactionType.Transfer, Quantity = quantity,
                        PreviousQuantity = destPrevQty, NewQuantity = destPrevQty + quantity,
                        Notes = $"Transfer In from Warehouse ID {warehouseEntityId}. {notes}",
                        CreatedByUserId = userId
                    });
                }
                else
                {
                    var stock = await _context.InventoryStocks
                        .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseEntityId == warehouseEntityId && s.BinId == actualBinId);

                    decimal previousQuantity = stock?.Quantity ?? 0m;
                    decimal newQuantity;

                    switch (transactionType)
                    {
                        case InventoryTransactionType.StockIn:
                        case InventoryTransactionType.PurchaseReceive:
                        case InventoryTransactionType.ReleaseReservation:
                            newQuantity = previousQuantity + quantity;
                            break;

                        case InventoryTransactionType.StockOut:
                        case InventoryTransactionType.OrderDispatch:
                        case InventoryTransactionType.Reservation:
                            newQuantity = previousQuantity - quantity;
                            if (newQuantity < 0)
                            {
                                ModelState.AddModelError(string.Empty, $"Insufficient stock. Current quantity is {previousQuantity}.");
                                await PopulateDropdownsAsync();
                                return View();
                            }
                            break;

                        case InventoryTransactionType.Adjustment:
                            newQuantity = quantity;
                            break;

                        default:
                            newQuantity = previousQuantity;
                            break;
                    }

                    if (stock == null)
                    {
                        stock = new InventoryStock
                        {
                            ProductId = productId, WarehouseEntityId = warehouseEntityId,
                            BinId = actualBinId, Quantity = newQuantity
                        };
                        _context.InventoryStocks.Add(stock);
                    }
                    else
                    {
                        stock.Quantity = newQuantity;
                    }

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = productId, WarehouseEntityId = warehouseEntityId,
                        TransactionType = transactionType, Quantity = quantity,
                        PreviousQuantity = previousQuantity, NewQuantity = newQuantity,
                        Notes = notes, CreatedByUserId = userId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Inventory adjustment completed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "An error occurred while processing the adjustment. Please try again.");
                await PopulateDropdownsAsync();
                return View();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /Inventory/GetBinsForWarehouse  — AJAX
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetBinsForWarehouse(int warehouseId)
        {
            var bins = await _context.Bins
                .AsNoTracking()
                .Where(b => b.WarehouseEntityId == warehouseId && !b.IsDeleted)
                .OrderBy(b => b.Code)
                .Select(b => new { id = b.Id, code = b.Code })
                .ToListAsync();

            return Json(bins);
        }

        // ─────────────────────────────────────────────────────────────
        // GET: /Inventory/Search — advanced search (CQRS)
        // ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Search(ProductSearchRequest searchRequest)
        {
            var query = new SearchProductsQuery { Request = searchRequest };
            var result = await _mediator.Send(query);

            // Populate filter dropdowns
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.Name)
                .Select(w => new { w.Id, w.Name })
                .ToListAsync();

            ViewBag.Categories  = new SelectList(categories, "Id", "Name", searchRequest.CategoryId);
            ViewBag.Warehouses  = new SelectList(warehouses, "Id", "Name", searchRequest.WarehouseId);
            ViewBag.SearchRequest = searchRequest;

            var pagedResult = result.Data ?? new PagedResult<ProductSearchResultDto>();
            return View(pagedResult);
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────
        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Products = new SelectList(
                await _context.Products.AsNoTracking()
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.Name)
                    .ToListAsync(),
                "Id", "Name");

            ViewBag.Warehouses = new SelectList(
                await _context.Warehouses.AsNoTracking()
                    .Where(w => !w.IsDeleted)
                    .OrderBy(w => w.Name)
                    .ToListAsync(),
                "Id", "Name");

            ViewBag.TransactionTypes = new SelectList(
                Enum.GetValues<InventoryTransactionType>()
                    .Select(t => new { Value = (int)t, Text = t.ToString() }),
                "Value", "Text");
        }
    }
}
