#nullable enable
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Warehouse.CQRS.Commands.Products;
using Warehouse.CQRS.Queries.Products;
using Warehouse.Data;
using Warehouse.Models.DTOs;

namespace Warehouse.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public ProductsController(IMediator mediator, ApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        // GET: /Products
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            const int pageSize = 20;

            var query = new GetAllProductsQuery
            {
                Page       = page,
                PageSize   = pageSize,
                SearchTerm = search,
                CategoryId = categoryId
            };
            var result = await _mediator.Send(query);

            var totalCount = await _context.Products
                .Where(p => !p.IsDeleted &&
                    (string.IsNullOrEmpty(search) ||
                     p.Name.Contains(search) ||
                     p.SKU.Contains(search) ||
                     (p.Barcode != null && p.Barcode.Contains(search))) &&
                    (!categoryId.HasValue || categoryId == 0 || p.CategoryId == categoryId))
                .CountAsync();

            await LoadCategoriesDropdown(categoryId);
            ViewBag.Search       = search;
            ViewBag.CategoryId   = categoryId;
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount   = totalCount;

            return View(result.Data ?? Enumerable.Empty<ProductDto>());
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Load inventory summary for this product
            var inventorySummary = await _context.InventoryStocks
                .AsNoTracking()
                .Where(s => s.ProductId == id && !s.IsDeleted)
                .Select(s => new InventoryStockDto
                {
                    Id                = s.Id,
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

            ViewBag.InventorySummary = inventorySummary;
            return View(result.Data);
        }

        // GET: /Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesDropdown();
            await LoadWarehousesDropdown();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesDropdown();
                await LoadWarehousesDropdown();
                return View();
            }

            // SKU uniqueness
            var skuExists = await _context.Products.AnyAsync(p => p.SKU == command.SKU && !p.IsDeleted);
            if (skuExists)
            {
                ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                await LoadCategoriesDropdown();
                await LoadWarehousesDropdown();
                return View();
            }

            // Barcode uniqueness (only if provided)
            if (!string.IsNullOrWhiteSpace(command.Barcode))
            {
                var barcodeExists = await _context.Products
                    .AnyAsync(p => p.Barcode == command.Barcode.Trim() && !p.IsDeleted);
                if (barcodeExists)
                {
                    ModelState.AddModelError("Barcode", "A product with this barcode already exists.");
                    await LoadCategoriesDropdown();
                    await LoadWarehousesDropdown();
                    return View();
                }
            }

            var result = await _mediator.Send(command);
            if (result.Success)
            {
                TempData["Success"] = "Product created successfully." +
                    (command.InitialQuantity > 0 ? $" Initial stock of {command.InitialQuantity:N0} units created." : "");
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;
            await LoadCategoriesDropdown();
            await LoadWarehousesDropdown();
            return View();
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            await LoadCategoriesDropdown(result.Data.CategoryId);
            return View(result.Data);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductCommand command)
        {
            command.Id = id;
            if (!ModelState.IsValid)
            {
                await LoadCategoriesDropdown(command.CategoryId);
                var dto = new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Barcode = command.Barcode, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId };
                return View(dto);
            }

            // SKU uniqueness (exclude self)
            var skuExists = await _context.Products.AnyAsync(p => p.SKU == command.SKU && p.Id != id && !p.IsDeleted);
            if (skuExists)
            {
                ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                await LoadCategoriesDropdown(command.CategoryId);
                var dto = new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Barcode = command.Barcode, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId };
                return View(dto);
            }

            // Barcode uniqueness (exclude self, only if provided)
            if (!string.IsNullOrWhiteSpace(command.Barcode))
            {
                var barcodeExists = await _context.Products
                    .AnyAsync(p => p.Barcode == command.Barcode.Trim() && p.Id != id && !p.IsDeleted);
                if (barcodeExists)
                {
                    ModelState.AddModelError("Barcode", "A product with this barcode already exists.");
                    await LoadCategoriesDropdown(command.CategoryId);
                    var dto = new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Barcode = command.Barcode, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId };
                    return View(dto);
                }
            }

            var result = await _mediator.Send(command);
            if (result.Success)
            {
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;
            await LoadCategoriesDropdown(command.CategoryId);
            return View(new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Barcode = command.Barcode, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId });
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id });
            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? "Product deleted successfully."
                : result.Message;
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesDropdown(int? selectedId = null)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }

        private async Task LoadWarehousesDropdown(int? selectedId = null)
        {
            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.Name)
                .Select(w => new { w.Id, w.Name })
                .ToListAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", selectedId);
        }
    }
}
