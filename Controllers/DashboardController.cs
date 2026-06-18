#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Data;
using Warehouse.Models.Enums;
using Warehouse.Models.ViewModels;

namespace Warehouse.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await BuildDashboardViewModel();

            if (User.IsInRole("Admin"))
                return View("Admin", vm);

            if (User.IsInRole("Manager"))
                return View("Manager", vm);

            if (User.IsInRole("Worker"))
                return View("Worker", vm);

            return View("Viewer", vm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var vm = await BuildDashboardViewModel();
            return View(vm);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Manager()
        {
            var vm = await BuildDashboardViewModel();
            return View(vm);
        }

        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> Worker()
        {
            var vm = await BuildDashboardViewModel();
            return View(vm);
        }

        public async Task<IActionResult> Viewer()
        {
            var vm = await BuildDashboardViewModel();
            return View(vm);
        }

        private async Task<DashboardViewModel> BuildDashboardViewModel()
        {
            var vm = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalWarehouses = await _context.Warehouses.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalInventoryStocks = await _context.InventoryStocks.CountAsync(),
                PendingOrdersCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                ActiveShipmentsCount = await _context.Shipments.CountAsync(s => s.Status == ShipmentStatus.InTransit),
            };

            // Low stock (quantity < 10)
            vm.LowStockCount = await _context.InventoryStocks.CountAsync(i => i.Quantity < 10);
            vm.LowStockProducts = await _context.InventoryStocks
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.WarehouseEntity)
                .Where(i => i.Quantity < 10)
                .OrderBy(i => i.Quantity)
                .Take(10)
                .Select(i => new LowStockInfo
                {
                    ProductName = i.Product.Name,
                    SKU = i.Product.SKU,
                    WarehouseName = i.WarehouseEntity.Name,
                    Quantity = i.Quantity
                })
                .ToListAsync();

            // Recent orders
            vm.RecentOrders = await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderInfo
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            // Recent transactions
            vm.RecentTransactions = await _context.InventoryTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .Include(t => t.WarehouseEntity)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new RecentTransactionInfo
                {
                    ProductName = t.Product != null ? t.Product.Name : "—",
                    WarehouseName = t.WarehouseEntity != null ? t.WarehouseEntity.Name : "—",
                    TransactionType = t.TransactionType.ToString(),
                    Quantity = t.Quantity,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            // Total inventory value
            vm.TotalInventoryValue = await _context.InventoryStocks
                .Include(i => i.Product)
                .SumAsync(i => i.Quantity * i.Product.UnitPrice);

            return vm;
        }
    }
}