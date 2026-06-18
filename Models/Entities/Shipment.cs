#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Warehouse.Models.Entities
{
    public class Shipment : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string TrackingNumber { get; set; } = string.Empty;

        [Required]
        public Warehouse.Models.Enums.ShipmentStatus Status { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
    }
}
