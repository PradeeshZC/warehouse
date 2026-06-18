#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Warehouse.Models.Identity;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;
using Warehouse.Data;

namespace Warehouse.Data.Configurations
{
    public static class RoleAndUserSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = provider.GetRequiredService<ApplicationDbContext>();
            var logger = provider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // ── 1. Seed Roles ──
                var roles = new[] { "Admin", "Manager", "Worker", "Viewer", "Staff" };
                foreach (var r in roles)
                {
                    if (!await roleManager.RoleExistsAsync(r))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(r));
                        if (!roleResult.Succeeded)
                        {
                            var errors = string.Join(", ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                            logger.LogError($"Failed to create role '{r}': {errors}");
                            throw new InvalidOperationException($"Failed to create role '{r}': {errors}");
                        }
                        logger.LogInformation($"Role '{r}' created successfully.");
                    }
                    else
                    {
                        logger.LogInformation($"Role '{r}' already exists.");
                    }
                }

                // ── 2. Seed Users ──
                await SeedUserAsync(userManager, logger, "admin@warehouse.local", "admin", "System Administrator", "Admin@12345", "Admin");
                await SeedUserAsync(userManager, logger, "manager@warehouse.local", "manager", "Warehouse Manager", "Manager@12345", "Manager");
                await SeedUserAsync(userManager, logger, "worker@warehouse.local", "worker", "Warehouse Worker", "Worker@12345", "Worker");
                await SeedUserAsync(userManager, logger, "viewer@warehouse.local", "viewer", "Read-Only Viewer", "Viewer@12345", "Viewer");

                // ── 3. Seed Categories ──
                if (!await context.Categories.AnyAsync())
                {
                    var categories = new[]
                    {
                        new Category { Name = "Electronics", Description = "Electronic devices and components" },
                        new Category { Name = "Office Supplies", Description = "Stationery, paper, and office equipment" },
                        new Category { Name = "Raw Materials", Description = "Industrial raw materials and chemicals" },
                        new Category { Name = "Packaging", Description = "Boxes, tape, bubble wrap, and shipping materials" },
                        new Category { Name = "Safety Equipment", Description = "PPE, fire extinguishers, and safety gear" },
                        new Category { Name = "Tools & Hardware", Description = "Hand tools, power tools, and hardware" },
                        new Category { Name = "Furniture", Description = "Office and warehouse furniture" },
                        new Category { Name = "Cleaning Supplies", Description = "Cleaning chemicals and janitorial equipment" },
                    };
                    context.Categories.AddRange(categories);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Categories seeded.");
                }

                // ── 4. Seed Warehouses & Bins ──
                if (!await context.Warehouses.AnyAsync())
                {
                    var wh1 = new WarehouseEntity { Name = "Main Warehouse", Location = "123 Industrial Ave, Building A" };
                    var wh2 = new WarehouseEntity { Name = "Secondary Warehouse", Location = "456 Commerce Blvd, Unit 7" };
                    var wh3 = new WarehouseEntity { Name = "Cold Storage", Location = "789 Logistics Park, Dock 3" };
                    context.Warehouses.AddRange(wh1, wh2, wh3);
                    await context.SaveChangesAsync();

                    // Bins
                    var bins = new[]
                    {
                        new Bin { Code = "A-01-01", Description = "Aisle A, Rack 1, Shelf 1", WarehouseEntityId = wh1.Id },
                        new Bin { Code = "A-01-02", Description = "Aisle A, Rack 1, Shelf 2", WarehouseEntityId = wh1.Id },
                        new Bin { Code = "A-02-01", Description = "Aisle A, Rack 2, Shelf 1", WarehouseEntityId = wh1.Id },
                        new Bin { Code = "B-01-01", Description = "Aisle B, Rack 1, Shelf 1", WarehouseEntityId = wh1.Id },
                        new Bin { Code = "B-01-02", Description = "Aisle B, Rack 1, Shelf 2", WarehouseEntityId = wh1.Id },
                        new Bin { Code = "S-01-01", Description = "Section 1, Bay 1", WarehouseEntityId = wh2.Id },
                        new Bin { Code = "S-01-02", Description = "Section 1, Bay 2", WarehouseEntityId = wh2.Id },
                        new Bin { Code = "S-02-01", Description = "Section 2, Bay 1", WarehouseEntityId = wh2.Id },
                        new Bin { Code = "C-01", Description = "Cold Zone 1", WarehouseEntityId = wh3.Id },
                        new Bin { Code = "C-02", Description = "Cold Zone 2", WarehouseEntityId = wh3.Id },
                    };
                    context.Bins.AddRange(bins);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Warehouses and bins seeded.");
                }

                // ── 5. Seed Suppliers ──
                if (!await context.Suppliers.AnyAsync())
                {
                    var suppliers = new[]
                    {
                        new Supplier { Name = "TechSource Inc.", Address = "100 Silicon Way, San Jose, CA", Phone = "408-555-0100", Email = "orders@techsource.com" },
                        new Supplier { Name = "Global Parts Co.", Address = "200 Trade Center, Houston, TX", Phone = "713-555-0200", Email = "sales@globalparts.com" },
                        new Supplier { Name = "SafetyFirst Ltd.", Address = "50 Guard Lane, Chicago, IL", Phone = "312-555-0050", Email = "info@safetyfirst.com" },
                        new Supplier { Name = "PackRight Solutions", Address = "75 Box Street, Atlanta, GA", Phone = "404-555-0075", Email = "supply@packright.com" },
                    };
                    context.Suppliers.AddRange(suppliers);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Suppliers seeded.");
                }

                // ── 6. Seed Products ──
                if (!await context.Products.AnyAsync())
                {
                    var cats = await context.Categories.ToListAsync();
                    var electronicsId = cats.First(c => c.Name == "Electronics").Id;
                    var officeId = cats.First(c => c.Name == "Office Supplies").Id;
                    var rawId = cats.First(c => c.Name == "Raw Materials").Id;
                    var packagingId = cats.First(c => c.Name == "Packaging").Id;
                    var safetyId = cats.First(c => c.Name == "Safety Equipment").Id;
                    var toolsId = cats.First(c => c.Name == "Tools & Hardware").Id;

                    var products = new[]
                    {
                        new Product { Name = "Wireless Mouse", SKU = "ELEC-001", Description = "Ergonomic wireless mouse with USB receiver", UnitPrice = 24.99m, CategoryId = electronicsId },
                        new Product { Name = "USB-C Hub 7-in-1", SKU = "ELEC-002", Description = "Multi-port USB-C adapter with HDMI, USB 3.0, SD card", UnitPrice = 49.99m, CategoryId = electronicsId },
                        new Product { Name = "LED Monitor 27\"", SKU = "ELEC-003", Description = "27-inch IPS LED monitor, 4K resolution", UnitPrice = 329.99m, CategoryId = electronicsId },
                        new Product { Name = "Mechanical Keyboard", SKU = "ELEC-004", Description = "RGB mechanical keyboard with Cherry MX switches", UnitPrice = 89.99m, CategoryId = electronicsId },
                        new Product { Name = "Webcam HD 1080p", SKU = "ELEC-005", Description = "Full HD webcam with built-in microphone", UnitPrice = 59.99m, CategoryId = electronicsId },
                        new Product { Name = "A4 Copy Paper (5000 sheets)", SKU = "OFFC-001", Description = "Premium white A4 paper, 80 GSM", UnitPrice = 34.50m, CategoryId = officeId },
                        new Product { Name = "Ballpoint Pen Box (50)", SKU = "OFFC-002", Description = "Blue ink ballpoint pens, medium point", UnitPrice = 12.99m, CategoryId = officeId },
                        new Product { Name = "Stapler Heavy Duty", SKU = "OFFC-003", Description = "Industrial stapler, 100 sheet capacity", UnitPrice = 28.00m, CategoryId = officeId },
                        new Product { Name = "Steel Sheet 4x8 ft", SKU = "RAW-001", Description = "Cold-rolled steel sheet, 16 gauge", UnitPrice = 185.00m, CategoryId = rawId },
                        new Product { Name = "Copper Wire Spool 500m", SKU = "RAW-002", Description = "14 AWG solid copper wire", UnitPrice = 245.00m, CategoryId = rawId },
                        new Product { Name = "Cardboard Box Large", SKU = "PACK-001", Description = "Heavy duty corrugated box, 24x18x18", UnitPrice = 3.50m, CategoryId = packagingId },
                        new Product { Name = "Bubble Wrap Roll 100m", SKU = "PACK-002", Description = "Standard bubble wrap, 12\" wide", UnitPrice = 22.00m, CategoryId = packagingId },
                        new Product { Name = "Packing Tape (6-pack)", SKU = "PACK-003", Description = "Clear packing tape, 2\" x 110 yards", UnitPrice = 15.99m, CategoryId = packagingId },
                        new Product { Name = "Safety Helmet", SKU = "SAFE-001", Description = "OSHA-compliant hard hat, adjustable", UnitPrice = 18.50m, CategoryId = safetyId },
                        new Product { Name = "Safety Goggles", SKU = "SAFE-002", Description = "Anti-fog splash-proof safety goggles", UnitPrice = 8.99m, CategoryId = safetyId },
                        new Product { Name = "Cordless Drill", SKU = "TOOL-001", Description = "20V cordless drill/driver with 2 batteries", UnitPrice = 129.99m, CategoryId = toolsId },
                        new Product { Name = "Socket Set 40-piece", SKU = "TOOL-002", Description = "Chrome vanadium socket set, metric & SAE", UnitPrice = 45.99m, CategoryId = toolsId },
                    };
                    context.Products.AddRange(products);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Products seeded.");
                }

                // ── 7. Seed Inventory Stocks ──
                if (!await context.InventoryStocks.AnyAsync())
                {
                    var prods = await context.Products.ToListAsync();
                    var whs = await context.Warehouses.ToListAsync();
                    var bins = await context.Bins.ToListAsync();
                    var wh1 = whs.First(w => w.Name == "Main Warehouse");
                    var wh2 = whs.First(w => w.Name == "Secondary Warehouse");

                    var stocks = new List<InventoryStock>();
                    foreach (var p in prods)
                    {
                        var bin = bins.FirstOrDefault(b => b.WarehouseEntityId == wh1.Id);
                        stocks.Add(new InventoryStock
                        {
                            ProductId = p.Id,
                            WarehouseEntityId = wh1.Id,
                            BinId = bin?.Id,
                            Quantity = Random.Shared.Next(5, 200)
                        });
                        // Some products also in wh2
                        if (Random.Shared.Next(0, 2) == 1)
                        {
                            var bin2 = bins.FirstOrDefault(b => b.WarehouseEntityId == wh2.Id);
                            stocks.Add(new InventoryStock
                            {
                                ProductId = p.Id,
                                WarehouseEntityId = wh2.Id,
                                BinId = bin2?.Id,
                                Quantity = Random.Shared.Next(2, 50)
                            });
                        }
                    }
                    context.InventoryStocks.AddRange(stocks);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Inventory stocks seeded.");
                }

                // ── 8. Seed Orders ──
                if (!await context.Orders.AnyAsync())
                {
                    var prods = await context.Products.ToListAsync();
                    var orders = new[]
                    {
                        new Order { OrderNumber = "ORD-2026-001", Status = OrderStatus.Delivered, TotalAmount = 524.97m },
                        new Order { OrderNumber = "ORD-2026-002", Status = OrderStatus.Shipped, TotalAmount = 329.99m },
                        new Order { OrderNumber = "ORD-2026-003", Status = OrderStatus.Confirmed, TotalAmount = 185.00m },
                        new Order { OrderNumber = "ORD-2026-004", Status = OrderStatus.Pending, TotalAmount = 89.99m },
                        new Order { OrderNumber = "ORD-2026-005", Status = OrderStatus.Pending, TotalAmount = 245.50m },
                    };
                    context.Orders.AddRange(orders);
                    await context.SaveChangesAsync();

                    // Order items
                    var p1 = prods.First(p => p.SKU == "ELEC-001");
                    var p2 = prods.First(p => p.SKU == "ELEC-003");
                    var p3 = prods.First(p => p.SKU == "RAW-001");
                    var items = new[]
                    {
                        new OrderItem { OrderId = orders[0].Id, ProductId = p1.Id, Quantity = 10, UnitPrice = p1.UnitPrice },
                        new OrderItem { OrderId = orders[0].Id, ProductId = p2.Id, Quantity = 1, UnitPrice = p2.UnitPrice },
                        new OrderItem { OrderId = orders[1].Id, ProductId = p2.Id, Quantity = 1, UnitPrice = p2.UnitPrice },
                        new OrderItem { OrderId = orders[2].Id, ProductId = p3.Id, Quantity = 1, UnitPrice = p3.UnitPrice },
                        new OrderItem { OrderId = orders[3].Id, ProductId = prods.First(p => p.SKU == "ELEC-004").Id, Quantity = 1, UnitPrice = 89.99m },
                    };
                    context.OrderItems.AddRange(items);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Orders seeded.");
                }

                // ── 9. Seed Purchase Orders ──
                if (!await context.PurchaseOrders.AnyAsync())
                {
                    var suppliers = await context.Suppliers.ToListAsync();
                    var pos = new[]
                    {
                        new PurchaseOrder { SupplierId = suppliers[0].Id, OrderNumber = "PO-2026-001", TotalAmount = 2499.90m, Status = PurchaseOrderStatus.Received },
                        new PurchaseOrder { SupplierId = suppliers[1].Id, OrderNumber = "PO-2026-002", TotalAmount = 1850.00m, Status = PurchaseOrderStatus.Approved },
                        new PurchaseOrder { SupplierId = suppliers[3].Id, OrderNumber = "PO-2026-003", TotalAmount = 350.00m, Status = PurchaseOrderStatus.Submitted },
                    };
                    context.PurchaseOrders.AddRange(pos);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Purchase orders seeded.");
                }

                // ── 10. Seed Shipments ──
                if (!await context.Shipments.AnyAsync())
                {
                    var orders = await context.Orders.ToListAsync();
                    var shipments = new[]
                    {
                        new Shipment { OrderId = orders[0].Id, TrackingNumber = "TRK-001-2026", Status = ShipmentStatus.Delivered },
                        new Shipment { OrderId = orders[1].Id, TrackingNumber = "TRK-002-2026", Status = ShipmentStatus.InTransit },
                    };
                    context.Shipments.AddRange(shipments);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Shipments seeded.");
                }

                // ── 11. Seed Inventory Transactions ──
                if (!await context.InventoryTransactions.AnyAsync())
                {
                    var stocks = await context.InventoryStocks.Include(s => s.Product).Take(5).ToListAsync();
                    var txs = new List<InventoryTransaction>();
                    foreach (var stock in stocks)
                    {
                        txs.Add(new InventoryTransaction
                        {
                            ProductId = stock.ProductId,
                            WarehouseEntityId = stock.WarehouseEntityId,
                            BinId = stock.BinId,
                            TransactionType = InventoryTransactionType.StockIn,
                            Quantity = stock.Quantity,
                            PreviousQuantity = 0,
                            NewQuantity = stock.Quantity,
                            ReferenceNumber = $"INIT-{stock.ProductId:D4}",
                            Notes = "Initial stock load",
                            CreatedByUserId = "system"
                        });
                    }
                    context.InventoryTransactions.AddRange(txs);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Inventory transactions seeded.");
                }

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding.");
                throw;
            }
        }

        private static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, ILogger logger,
            string email, string username, string fullName, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = username, Email = email, FullName = fullName };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    logger.LogError($"Failed to create user '{email}': {errors}");
                    return;
                }
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation($"User '{email}' created with role '{role}'.");
            }
            else
            {
                logger.LogInformation($"User '{email}' already exists.");
            }
        }
    }
}
