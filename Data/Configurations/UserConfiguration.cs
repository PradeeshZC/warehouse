#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.UserName).HasMaxLength(150).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(254).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.RoleType).IsRequired();

            // Unique constraints
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserName).IsUnique();

            // Relationship configured in RoleConfiguration
        }
    }
}
