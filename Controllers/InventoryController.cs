#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Warehouse.Data;
using Warehouse.Models.DTOs;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;

namespace Warehouse.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Inventory
        public async Task<IActionResult> Index(int? warehouseId, int? binId)
        {
            var query = _context.InventoryStocks.AsQueryable();

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                query = query.Where(s => s.WarehouseEntityId == warehouseId.Value);
            }

            if (binId.HasValue && binId.Value > 0)
            {
                query = query.Where(s => s.BinId == binId.Value);
            }

            var stocks = await query
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.WarehouseEntity)
                .Include(s => s.Bin)
                .OrderBy(s => s.Product.Name)
                .Select(s => new InventoryStockDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    ProductSKU = s.Product.SKU,
                    WarehouseEntityId = s.WarehouseEntityId,
                    WarehouseName = s.WarehouseEntity.Name,
                    BinId = s.BinId,
                    BinCode = s.Bin != null ? s.Bin.Code : null,
                    Quantity = s.Quantity
                })
                .ToListAsync();

            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .ToListAsync();

            ViewBag.WarehouseId = warehouseId;
            ViewBag.BinId = binId;
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", warehouseId);

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                var bins = await _context.Bins
                    .AsNoTracking()
                    .Where(b => b.WarehouseEntityId == warehouseId.Value)
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

        // GET: /Inventory/Transactions
        public async Task<IActionResult> Transactions()
        {
            var transactions = await _context.InventoryTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .Include(t => t.WarehouseEntity)
                .OrderByDescending(t => t.CreatedAt)
                .Take(100)
                .Select(t => new InventoryTransactionDto
                {
                    InventoryTransactionId = t.InventoryTransactionId,
                    ProductId = t.ProductId,
                    ProductName = t.Product != null ? t.Product.Name : string.Empty,
                    WarehouseEntityId = t.WarehouseEntityId,
                    WarehouseName = t.WarehouseEntity != null ? t.WarehouseEntity.Name : string.Empty,
                    TransactionType = t.TransactionType,
                    Quantity = t.Quantity,
                    PreviousQuantity = t.PreviousQuantity,
                    NewQuantity = t.NewQuantity,
                    ReferenceNumber = t.ReferenceNumber,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return View(transactions);
        }

        // GET: /Inventory/Adjust
        public async Task<IActionResult> Adjust()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBinsForWarehouse(int warehouseId)
        {
            var bins = await _context.Bins
                .AsNoTracking()
                .Where(b => b.WarehouseEntityId == warehouseId)
                .OrderBy(b => b.Code)
                .Select(b => new { id = b.Id, code = b.Code })
                .ToListAsync();

            return Json(bins);
        }

        // POST: /Inventory/Adjust
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

            // If it is a transfer, validate destination inputs
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

                    // 1. Process Debit (Source Warehouse/Bin)
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

                    // Log source debit transaction
                    var debitTx = new InventoryTransaction
                    {
                        ProductId = productId,
                        WarehouseEntityId = warehouseEntityId,
                        TransactionType = InventoryTransactionType.Transfer,
                        Quantity = quantity,
                        PreviousQuantity = sourcePrevQty,
                        NewQuantity = sourcePrevQty - quantity,
                        Notes = $"Transfer Out to Warehouse ID {destWarehouseId} (Bin: {(actualBinId == null ? "N/A" : actualBinId.Value.ToString())}). Notes: {notes}",
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(debitTx);

                    // 2. Process Credit (Destination Warehouse/Bin)
                    int? actualDestBinId = (destinationBinId.HasValue && destinationBinId.Value > 0) ? destinationBinId.Value : null;
                    var destStock = await _context.InventoryStocks
                        .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseEntityId == destWarehouseId && s.BinId == actualDestBinId);

                    decimal destPrevQty = destStock?.Quantity ?? 0m;

                    if (destStock == null)
                    {
                        destStock = new InventoryStock
                        {
                            ProductId = productId,
                            WarehouseEntityId = destinationWarehouseEntityId.Value,
                            BinId = actualDestBinId,
                            Quantity = quantity
                        };
                        _context.InventoryStocks.Add(destStock);
                    }
                    else
                    {
                        destStock.Quantity += quantity;
                    }

                    // Log destination credit transaction
                    var creditTx = new InventoryTransaction
                    {
                        ProductId = productId,
                        WarehouseEntityId = destinationWarehouseEntityId.Value,
                        TransactionType = InventoryTransactionType.Transfer,
                        Quantity = quantity,
                        PreviousQuantity = destPrevQty,
                        NewQuantity = destPrevQty + quantity,
                        Notes = $"Transfer In from Warehouse ID {warehouseEntityId} (Bin: {(actualDestBinId == null ? "N/A" : actualDestBinId.Value.ToString())}). Notes: {notes}",
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(creditTx);
                }
                else
                {
                    // Find or create inventory stock record under specific bin
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
                            ProductId = productId,
                            WarehouseEntityId = warehouseEntityId,
                            BinId = actualBinId,
                            Quantity = newQuantity
                        };
                        _context.InventoryStocks.Add(stock);
                    }
                    else
                    {
                        stock.Quantity = newQuantity;
                    }

                    // Create transaction record
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = productId,
                        WarehouseEntityId = warehouseEntityId,
                        TransactionType = transactionType,
                        Quantity = quantity,
                        PreviousQuantity = previousQuantity,
                        NewQuantity = newQuantity,
                        Notes = notes,
                        CreatedByUserId = userId
                    };
                    _context.InventoryTransactions.Add(inventoryTransaction);
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

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Products = new SelectList(
                await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync(),
                "Id", "Name");

            ViewBag.Warehouses = new SelectList(
                await _context.Warehouses.AsNoTracking().OrderBy(w => w.Name).ToListAsync(),
                "Id", "Name");

            ViewBag.TransactionTypes = new SelectList(
                Enum.GetValues<InventoryTransactionType>()
                    .Select(t => new { Value = (int)t, Text = t.ToString() }),
                "Value", "Text");
        }
    }
}
