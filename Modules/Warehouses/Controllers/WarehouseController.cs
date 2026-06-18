#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Warehouse.CQRS.Queries.Products;
using Warehouse.Modules.Warehouses.DTOs;

namespace Warehouse.Modules.Warehouses.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly IMediator _mediator;

        public WarehouseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
