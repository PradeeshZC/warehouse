#nullable enable
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Configurations
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyConfigurationsFromAssembly(this ModelBuilder builder, Assembly assembly)
        {
            // Already provided by EF Core, but keep for explicitness
            builder.ApplyConfigurationsFromAssembly(assembly);
        }

        public static void ApplyGlobalQueryFilters(this ModelBuilder builder)
        {
            // Apply soft-delete filter to all entities inheriting BaseEntity
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ModelBuilderExtensions).GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { builder });
                }
            }
        }

        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        public static void ApplySeeders(this ModelBuilder builder)
        {
            // Seed domain roles with deterministic timestamps (required for EF Core HasData)
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            builder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", CreatedAt = seedDate },
                new Role { Id = 2, Name = "Manager", CreatedAt = seedDate },
                new Role { Id = 3, Name = "Worker", CreatedAt = seedDate },
                new Role { Id = 4, Name = "Viewer", CreatedAt = seedDate }
            );
        }
    }
}
