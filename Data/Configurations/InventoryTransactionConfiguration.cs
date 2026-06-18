#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;
using Warehouse.Models.Enums;

namespace Warehouse.Data.Configurations
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(it => it.InventoryTransactionId);

            builder.Property(it => it.Quantity).HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(it => it.PreviousQuantity).HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(it => it.NewQuantity).HasColumnType("decimal(18,4)").IsRequired();

            builder.HasIndex(it => it.ProductId);
            builder.HasIndex(it => it.WarehouseEntityId);
            builder.HasIndex(it => it.CreatedAt);
            builder.HasIndex(it => it.TransactionType);

            builder.HasOne(it => it.Product)
                .WithMany()
                .HasForeignKey(it => it.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(it => it.WarehouseEntity)
                .WithMany()
                .HasForeignKey(it => it.WarehouseEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(it => it.Bin)
                .WithMany()
                .HasForeignKey(it => it.BinId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
