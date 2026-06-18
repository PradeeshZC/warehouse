#nullable enable
using Warehouse.Data;
using Warehouse.Models.Entities;

namespace Warehouse.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.SampleEntities.Any())
            {
                context.SampleEntities.Add(new SampleEntity { Name = "Initial Sample" });
                await context.SaveChangesAsync();
            }
        }
    }
}
