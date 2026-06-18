#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Warehouse.Models.Entities
{
    public class Bin : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        // FK
        [ForeignKey("WarehouseEntity")]
        public int WarehouseEntityId { get; set; }

        // Navigation
        public WarehouseEntity WarehouseEntity { get; set; } = null!;
        public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
    }
}
