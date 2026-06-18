#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.TrackingNumber).HasMaxLength(100).IsRequired();
            builder.Property(s => s.Status).IsRequired();

            builder.HasIndex(s => s.OrderId);

            builder.HasOne(s => s.Order)
                .WithMany(o => o.Shipments)
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
