#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Warehouse.Models.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
