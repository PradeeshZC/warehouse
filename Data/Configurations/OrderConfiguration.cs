#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(o => o.Status).IsRequired();

            builder.HasIndex(o => o.OrderNumber).IsUnique(false);
        }
    }
}
