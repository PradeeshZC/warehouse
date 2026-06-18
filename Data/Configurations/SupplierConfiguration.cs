#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).HasMaxLength(250).IsRequired();
            builder.Property(s => s.Address).HasMaxLength(500);
            builder.Property(s => s.Phone).HasMaxLength(50);
            builder.Property(s => s.Email).HasMaxLength(254);

            builder.HasIndex(s => s.Name);
        }
    }
}
