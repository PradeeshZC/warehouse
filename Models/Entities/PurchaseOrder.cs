#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Warehouse.Models.Entities;

namespace Warehouse.Models.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        [Required]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public Warehouse.Models.Enums.PurchaseOrderStatus Status { get; set; }

        // Navigation
        public Supplier Supplier { get; set; } = null!;
        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
