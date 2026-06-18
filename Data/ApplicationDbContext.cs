#nullable enable
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Warehouse.Models.Identity;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;
using Warehouse.Data.Configurations;

namespace Warehouse.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        // Domain role/user tables (separate from Identity tables)
        public DbSet<Role> DomainRoles { get; set; } = null!;
        public DbSet<User> DomainUsers { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<WarehouseEntity> Warehouses { get; set; } = null!;
        public DbSet<Bin> Bins { get; set; } = null!;
        public DbSet<InventoryStock> InventoryStocks { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Shipment> Shipments { get; set; } = null!;

        // Inventory transactions (history)
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;

        // legacy/sample
        public DbSet<SampleEntity> SampleEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from Data/Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Apply global query filters for soft-delete
            ModelBuilderExtensions.ApplyGlobalQueryFilters(modelBuilder);

            // Seed placeholders
            ModelBuilderExtensions.ApplySeeders(modelBuilder);

            // Note: Identity tables use default EF Core Identity table names (AspNetUsers, AspNetRoles, ...)
            // Avoid mapping Identity tables to the same names as domain tables to prevent conflicts with domain User entity.
        }

        public override int SaveChanges()
        {
            SetTimestampsAndSoftDelete();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            SetTimestampsAndSoftDelete();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestampsAndSoftDelete()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.IsDeleted = false;
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Deleted)
                {
                    // Soft delete: convert to modified and set IsDeleted
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
