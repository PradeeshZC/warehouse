#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Warehouse.Models.Enums;

namespace Warehouse.Models.Entities
{
    public class InventoryTransaction : BaseEntity
    {
        [Key]
        public int InventoryTransactionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseEntityId { get; set; }

        public int? BinId { get; set; }

        [Required]
        public InventoryTransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal PreviousQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal NewQuantity { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        // Navigation
        public Product? Product { get; set; }
        public WarehouseEntity? WarehouseEntity { get; set; }
        public Bin? Bin { get; set; }
    }
}
