#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Entities
{
    public class InventoryStock : BaseEntity
    {
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey("WarehouseEntity")]
        public int WarehouseEntityId { get; set; }

        [ForeignKey("Bin")]
        public int? BinId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Quantity currently reserved for pending orders / active reservations.
        /// Cannot be reduced below this value during stock adjustment.
        /// Updated by Reservation / ReleaseReservation transaction handlers.
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal ReservedQuantity { get; set; } = 0;

        /// <summary>Available stock = Quantity - ReservedQuantity</summary>
        [NotMapped]
        public decimal AvailableQuantity => Quantity - ReservedQuantity;

        // Navigation
        public Product Product { get; set; } = null!;
        public WarehouseEntity WarehouseEntity { get; set; } = null!;
        public Bin? Bin { get; set; }
    }
}
