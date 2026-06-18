#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("PurchaseOrders");

            builder.HasKey(po => po.Id);

            builder.Property(po => po.OrderNumber).HasMaxLength(50).IsRequired();
            builder.Property(po => po.TotalAmount).HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(po => po.Status).IsRequired();

            builder.HasIndex(po => po.OrderNumber).IsUnique(false);
            builder.HasIndex(po => po.SupplierId);

            builder.HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
