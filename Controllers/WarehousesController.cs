#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Data;
using Warehouse.Models.DTOs;
using Warehouse.Models.Entities;

namespace Warehouse.Controllers
{
    [Authorize]
    public class WarehousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Warehouses
        public async Task<IActionResult> Index()
        {
            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Location = w.Location,
                    BinCount = w.Bins.Count(b => !b.IsDeleted),
                    TotalStockQuantity = w.InventoryStocks.Sum(s => s.Quantity)
                })
                .ToListAsync();

            return View(warehouses);
        }

        // GET: /Warehouses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .Include(w => w.Bins)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
            {
                TempData["Error"] = "Warehouse not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get bins details
            var bins = warehouse.Bins
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Code)
                .Select(b => new BinDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Description = b.Description,
                    WarehouseEntityId = b.WarehouseEntityId,
                    WarehouseName = warehouse.Name,
                    StockCount = _context.InventoryStocks.Count(s => s.BinId == b.Id),
                    TotalQuantity = _context.InventoryStocks.Where(s => s.BinId == b.Id).Sum(s => s.Quantity)
                })
                .ToList();

            // Get stocks inside this warehouse
            var stocks = await _context.InventoryStocks
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.Bin)
                .Where(s => s.WarehouseEntityId == id)
                .OrderBy(s => s.Product.Name)
                .Select(s => new InventoryStockDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    ProductSKU = s.Product.SKU,
                    WarehouseEntityId = s.WarehouseEntityId,
                    WarehouseName = warehouse.Name,
                    BinId = s.BinId,
                    BinCode = s.Bin != null ? s.Bin.Code : "N/A",
                    Quantity = s.Quantity
                })
                .ToListAsync();

            ViewBag.Bins = bins;
            ViewBag.Stocks = stocks;

            return View(warehouse);
        }

        // GET: /Warehouses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Warehouses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseEntity warehouse)
        {
            if (string.IsNullOrWhiteSpace(warehouse.Name))
            {
                ModelState.AddModelError("Name", "Warehouse name is required.");
                return View(warehouse);
            }

            // Name uniqueness check
            var exists = await _context.Warehouses.AnyAsync(w => w.Name == warehouse.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "A warehouse with this name already exists.");
                return View(warehouse);
            }

            if (ModelState.IsValid)
            {
                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Warehouse created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        // GET: /Warehouses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                TempData["Error"] = "Warehouse not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        // POST: /Warehouses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarehouseEntity warehouse)
        {
            if (id != warehouse.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(warehouse.Name))
            {
                ModelState.AddModelError("Name", "Warehouse name is required.");
                return View(warehouse);
            }

            // Name uniqueness check (excluding current warehouse)
            var exists = await _context.Warehouses.AnyAsync(w => w.Name == warehouse.Name && w.Id != id);
            if (exists)
            {
                ModelState.AddModelError("Name", "A warehouse with this name already exists.");
                return View(warehouse);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warehouse);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Warehouse updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await WarehouseExists(warehouse.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        // GET: /Warehouses/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
            {
                TempData["Error"] = "Warehouse not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if there is inventory stock in this warehouse
            var hasStock = await _context.InventoryStocks.AnyAsync(s => s.WarehouseEntityId == id && s.Quantity > 0);
            ViewBag.HasStock = hasStock;

            return View(warehouse);
        }

        // POST: /Warehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                TempData["Error"] = "Warehouse not found.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deletion if there is stock
            var hasStock = await _context.InventoryStocks.AnyAsync(s => s.WarehouseEntityId == id && s.Quantity > 0);
            if (hasStock)
            {
                TempData["Error"] = "Cannot delete warehouse containing active inventory stock.";
                return RedirectToAction(nameof(Index));
            }

            // Soft-delete the warehouse and its associated bins
            var bins = await _context.Bins.Where(b => b.WarehouseEntityId == id).ToListAsync();
            foreach (var bin in bins)
            {
                _context.Bins.Remove(bin);
            }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Warehouse and its bins deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Warehouses/AddBin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBin(int warehouseId, string code, string? description)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "Bin code is required.";
                return RedirectToAction(nameof(Details), new { id = warehouseId });
            }

            var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId);
            if (!warehouseExists)
            {
                TempData["Error"] = "Warehouse not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check unique bin code in this warehouse
            var exists = await _context.Bins.AnyAsync(b => b.WarehouseEntityId == warehouseId && b.Code == code);
            if (exists)
            {
                TempData["Error"] = $"A bin with code '{code}' already exists in this warehouse.";
                return RedirectToAction(nameof(Details), new { id = warehouseId });
            }

            var bin = new Bin
            {
                WarehouseEntityId = warehouseId,
                Code = code.Trim().ToUpper(),
                Description = description
            };

            _context.Bins.Add(bin);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bin added successfully.";
            return RedirectToAction(nameof(Details), new { id = warehouseId });
        }

        private async Task<bool> WarehouseExists(int id)
        {
            return await _context.Warehouses.AnyAsync(e => e.Id == id);
        }
    }
}
