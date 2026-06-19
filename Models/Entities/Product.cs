#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Warehouse.Models.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(300)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>Unique barcode (EAN-13, UPC, QR, etc.). Nullable — not all products have a barcode.</summary>
        [StringLength(100)]
        public string? Barcode { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        // FK
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        // Navigation
        public Category Category { get; set; } = null!;
        public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
