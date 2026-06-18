#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<WarehouseEntity>
    {
        public void Configure(EntityTypeBuilder<WarehouseEntity> builder)
        {
            builder.ToTable("Warehouses");

            builder.HasKey(w => w.Id);
            builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
            builder.Property(w => w.Location).HasMaxLength(500);

            builder.HasIndex(w => w.Name);

            builder.HasMany(w => w.Bins)
                .WithOne(b => b.WarehouseEntity)
                .HasForeignKey(b => b.WarehouseEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
