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

            builder.Property(i => i.Quantity).HasColumnType("decimal(18,4)").IsRequired();

            builder.HasIndex(i => new { i.ProductId, i.WarehouseEntityId, i.BinId });

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
