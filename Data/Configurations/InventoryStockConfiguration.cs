#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class InventoryStockConfiguration : IEntityTypeConfiguration<InventoryStock>
    {
        public void Configure(EntityTypeBuilder<InventoryStock> builder)
        {
            builder.ToTable("InventoryStocks");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                   .HasColumnType("decimal(18,4)")
                   .IsRequired();

            builder.Property(i => i.ReservedQuantity)
                   .HasColumnType("decimal(18,4)")
                   .HasDefaultValue(0m)
                   .IsRequired();

            // Composite index for the most common query pattern: find stock for a product in a specific location
            builder.HasIndex(i => new { i.ProductId, i.WarehouseEntityId, i.BinId })
                   .HasDatabaseName("IX_InventoryStocks_Product_Warehouse_Bin");

            // Individual indexes for filter-only queries
            builder.HasIndex(i => i.ProductId).HasDatabaseName("IX_InventoryStocks_ProductId");
            builder.HasIndex(i => i.WarehouseEntityId).HasDatabaseName("IX_InventoryStocks_WarehouseEntityId");
            builder.HasIndex(i => i.BinId).HasDatabaseName("IX_InventoryStocks_BinId");

            builder.HasOne(i => i.Product)
                .WithMany(p => p.InventoryStocks)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.WarehouseEntity)
                .WithMany(w => w.InventoryStocks)
                .HasForeignKey(i => i.WarehouseEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Bin)
                .WithMany(b => b.InventoryStocks)
                .HasForeignKey(i => i.BinId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
