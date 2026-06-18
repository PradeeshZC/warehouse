#nullable enable
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.CQRS.Commands.Categories;
using Warehouse.CQRS.Queries.Categories;

namespace Warehouse.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var response = await _mediator.Send(new GetAllCategoriesQuery());
            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                return View(Enumerable.Empty<Warehouse.Models.DTOs.CategoryDto>());
            }

            return View(response.Data);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Name is required.");
                return View();
            }

            var command = new CreateCategoryCommand
            {
                Name = name,
                Description = description
            };

            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                return View();
            }

            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            if (!response.Success || response.Data == null)
            {
                TempData["Error"] = response.Message ?? "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Id"] = response.Data.Id;
            ViewData["Name"] = response.Data.Name;
            ViewData["Description"] = response.Data.Description;

            return View();
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Name is required.");
                ViewData["Id"] = id;
                ViewData["Name"] = name;
                ViewData["Description"] = description;
                return View();
            }

            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = name,
                Description = description
            };

            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                ViewData["Id"] = id;
                ViewData["Name"] = name;
                ViewData["Description"] = description;
                return View();
            }

            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            if (!response.Success || response.Data == null)
            {
                TempData["Error"] = response.Message ?? "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var command = new DeleteCategoryCommand { Id = id };
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
