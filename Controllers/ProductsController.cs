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
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var query = new GetAllProductsQuery { Page = page, PageSize = 20, SearchTerm = search };
            var result = await _mediator.Send(query);

            var totalCount = await _context.Products
                .Where(p => string.IsNullOrEmpty(search)
                    || p.Name.Contains(search)
                    || p.SKU.Contains(search))
                .CountAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);

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
            return View(result.Data);
        }

        // GET: /Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesDropdown();
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
                return View();
            }

            // Check SKU uniqueness
            var skuExists = await _context.Products.AnyAsync(p => p.SKU == command.SKU);
            if (skuExists)
            {
                ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                await LoadCategoriesDropdown();
                return View();
            }

            var result = await _mediator.Send(command);
            if (result.Success)
            {
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;
            await LoadCategoriesDropdown();
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
                var dto = new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId };
                return View(dto);
            }

            // Check SKU uniqueness (exclude current product)
            var skuExists = await _context.Products.AnyAsync(p => p.SKU == command.SKU && p.Id != id);
            if (skuExists)
            {
                ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                await LoadCategoriesDropdown(command.CategoryId);
                var dto = new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId };
                return View(dto);
            }

            var result = await _mediator.Send(command);
            if (result.Success)
            {
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;
            await LoadCategoriesDropdown(command.CategoryId);
            return View(new ProductDto { Id = id, Name = command.Name, SKU = command.SKU, Description = command.Description, UnitPrice = command.UnitPrice, CategoryId = command.CategoryId });
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
            if (result.Success)
            {
                TempData["Success"] = "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesDropdown(int? selectedId = null)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }
    }
}
