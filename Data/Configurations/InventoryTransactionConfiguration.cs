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

            builder.Property(it => it.Reason).HasMaxLength(500);
            builder.Property(it => it.Notes).HasMaxLength(1000);
            builder.Property(it => it.ReferenceNumber).HasMaxLength(100);
            builder.Property(it => it.CreatedByUserId).HasMaxLength(450);

            // Performance indexes
            builder.HasIndex(it => it.ProductId).HasDatabaseName("IX_InventoryTransactions_ProductId");
            builder.HasIndex(it => it.WarehouseEntityId).HasDatabaseName("IX_InventoryTransactions_WarehouseEntityId");
            builder.HasIndex(it => it.CreatedAt).HasDatabaseName("IX_InventoryTransactions_CreatedAt");
            builder.HasIndex(it => it.TransactionType).HasDatabaseName("IX_InventoryTransactions_TransactionType");

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
