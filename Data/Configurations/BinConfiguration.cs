#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class BinConfiguration : IEntityTypeConfiguration<Bin>
    {
        public void Configure(EntityTypeBuilder<Bin> builder)
        {
            builder.ToTable("Bins");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Code).HasMaxLength(100).IsRequired();
            builder.Property(b => b.Description).HasMaxLength(250);

            builder.HasIndex(b => new { b.WarehouseEntityId, b.Code }).IsUnique();

            builder.HasOne(b => b.WarehouseEntity)
                .WithMany(w => w.Bins)
                .HasForeignKey(b => b.WarehouseEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
