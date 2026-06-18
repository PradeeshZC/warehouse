#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.UnitPrice).HasColumnType("decimal(18,4)").IsRequired();

            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.CategoryId);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // SKU uniqueness enforced via Fluent API
            builder.Property(p => p.SKU).HasMaxLength(100).IsRequired();
            builder.HasIndex(p => p.SKU).IsUnique();
        }
    }
}
