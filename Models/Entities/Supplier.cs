#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Warehouse.Models.Entities
{
    public class Supplier : BaseEntity
    {
        [Required]
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        // Navigation
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}
