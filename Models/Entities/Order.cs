#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Warehouse.Models.Entities
{
    public class Order : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Warehouse.Models.Enums.OrderStatus Status { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmount { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
